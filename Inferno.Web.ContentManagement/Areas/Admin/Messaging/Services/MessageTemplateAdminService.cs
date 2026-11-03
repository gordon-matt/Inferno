using Ardalis.Result;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Messaging.Services;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;

public sealed record MessageTemplateSummary(int Id, string Name, string Subject, bool Enabled, DateTime? DateModifiedUtc);

/// <summary>The template's name, subject and switch; the message itself is edited on the design page.</summary>
public sealed class MessageTemplateDetails
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Subject { get; set; }

    public bool Enabled { get; set; } = true;
}

public sealed record MessageTemplateDocument(int Id, string Name, string Subject, bool Enabled, string Body);

/// <summary>
/// Admin operations for message templates. Templates are edited in the default language: sending
/// falls back to that version whenever no translation exists.
/// </summary>
public interface IMessageTemplateAdminService
{
    Task<IReadOnlyList<MessageTemplateSummary>> ListAsync(int tenantId, string search);

    Task<Result<MessageTemplateDetails>> GetDetailsAsync(int tenantId, int id);

    Task<Result<MessageTemplateDocument>> GetDocumentAsync(int tenantId, int id);

    Task<Result<MessageTemplateDetails>> SaveDetailsAsync(int tenantId, MessageTemplateDetails details);

    Task<Result> SaveBodyAsync(int tenantId, int id, string body);

    Task<Result> DeleteAsync(int tenantId, int id);
}

public class MessageTemplateAdminService(
    IMessageTemplateService templateService,
    IMessageTemplateVersionService versionService) : IMessageTemplateAdminService
{
    private const string EDITOR = "PageKrafter";

    public Task<IReadOnlyList<MessageTemplateSummary>> ListAsync(int tenantId, string search)
    {
        List<MessageTemplate> templates;
        using (var connection = templateService.OpenConnection())
        {
            var query = connection.Query(template => template.TenantId == tenantId);
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                query = query.Where(template => template.Name.Contains(term));
            }

            templates = query.OrderBy(template => template.Name).ToList();
        }

        var ids = templates.Select(template => template.Id).ToList();
        Dictionary<int, MessageTemplateVersion> versions;
        using (var connection = versionService.OpenConnection())
        {
            versions = connection
                .Query(version => ids.Contains(version.MessageTemplateId) && version.CultureCode == null)
                .ToList()
                .GroupBy(version => version.MessageTemplateId)
                .ToDictionary(group => group.Key, group => group.First());
        }

        IReadOnlyList<MessageTemplateSummary> summaries = templates
            .Select(template =>
            {
                var version = versions.GetValueOrDefault(template.Id);
                return new MessageTemplateSummary(template.Id, template.Name, version?.Subject, template.Enabled, version?.DateModifiedUtc);
            })
            .ToList();

        return Task.FromResult(summaries);
    }

    public async Task<Result<MessageTemplateDetails>> GetDetailsAsync(int tenantId, int id)
    {
        var template = await FindAsync(tenantId, id);
        if (template is null)
        {
            return Result<MessageTemplateDetails>.NotFound();
        }

        var version = versionService.FindOne(template.Id, null);
        return Result<MessageTemplateDetails>.Success(new MessageTemplateDetails
        {
            Id = template.Id,
            Name = template.Name,
            Subject = version?.Subject,
            Enabled = template.Enabled,
        });
    }

    public async Task<Result<MessageTemplateDocument>> GetDocumentAsync(int tenantId, int id)
    {
        var template = await FindAsync(tenantId, id);
        if (template is null)
        {
            return Result<MessageTemplateDocument>.NotFound();
        }

        var version = versionService.FindOne(template.Id, null);
        return Result<MessageTemplateDocument>.Success(new MessageTemplateDocument(
            template.Id,
            template.Name,
            version?.Subject,
            template.Enabled,
            MessageTemplateDocuments.ForEditor(version?.Data)));
    }

    public async Task<Result<MessageTemplateDetails>> SaveDetailsAsync(int tenantId, MessageTemplateDetails details)
    {
        string name = details.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return Result<MessageTemplateDetails>.Invalid(new ValidationError { Identifier = nameof(MessageTemplateDetails.Name), ErrorMessage = "Enter a name." });
        }

        bool nameTaken;
        using (var connection = templateService.OpenConnection())
        {
            nameTaken = connection
                .Query(template => template.TenantId == tenantId && template.Name == name && template.Id != details.Id)
                .Any();
        }

        if (nameTaken)
        {
            return Result<MessageTemplateDetails>.Invalid(new ValidationError { Identifier = nameof(MessageTemplateDetails.Name), ErrorMessage = "Another template already uses this name." });
        }

        MessageTemplate template;
        if (details.Id == 0)
        {
            template = await templateService.InsertAsync(new MessageTemplate
            {
                TenantId = tenantId,
                Name = name,
                Editor = EDITOR,
                Enabled = details.Enabled,
            });
        }
        else
        {
            template = await FindAsync(tenantId, details.Id);
            if (template is null)
            {
                return Result<MessageTemplateDetails>.NotFound();
            }

            template.Name = name;
            template.Enabled = details.Enabled;
            template.Editor = EDITOR;
            await templateService.UpdateAsync(template);
        }

        await SaveVersionAsync(template.Id, version => version.Subject = details.Subject?.Trim() ?? string.Empty);

        details.Id = template.Id;
        details.Name = name;
        return Result<MessageTemplateDetails>.Success(details);
    }

    public async Task<Result> SaveBodyAsync(int tenantId, int id, string body)
    {
        var template = await FindAsync(tenantId, id);
        if (template is null)
        {
            return Result.NotFound();
        }

        await SaveVersionAsync(template.Id, version => version.Data = body ?? string.Empty);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int tenantId, int id)
    {
        var template = await FindAsync(tenantId, id);
        if (template is null)
        {
            return Result.NotFound();
        }

        await versionService.DeleteAsync(version => version.MessageTemplateId == template.Id);
        await templateService.DeleteAsync(template);
        return Result.Success();
    }

    private async Task<MessageTemplate> FindAsync(int tenantId, int id)
    {
        var template = await templateService.FindOneAsync(id);
        return template is not null && template.TenantId == tenantId ? template : null;
    }

    /// <summary>Applies <paramref name="change"/> to the default-language version, creating it the first time.</summary>
    private async Task SaveVersionAsync(int templateId, Action<MessageTemplateVersion> change)
    {
        MessageTemplateVersion version;
        using (var connection = versionService.OpenConnection())
        {
            version = connection
                .Query(candidate => candidate.MessageTemplateId == templateId && candidate.CultureCode == null)
                .FirstOrDefault();
        }

        var now = DateTime.UtcNow;
        if (version is null)
        {
            version = new MessageTemplateVersion
            {
                MessageTemplateId = templateId,
                Subject = string.Empty,
                Data = MessageTemplateDocuments.ForEditor(null),
                DateCreatedUtc = now,
            };
            change(version);
            version.DateModifiedUtc = now;
            await versionService.InsertAsync(version);
            return;
        }

        change(version);
        version.DateModifiedUtc = now;
        await versionService.UpdateAsync(version);
    }
}
