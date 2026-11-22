using Ardalis.Result;
using Inferno.Web.Messaging;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Messaging.Services;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;

/// <summary>A template row for the list, using the default-language subject.</summary>
/// <param name="Id">The template identifier.</param>
/// <param name="Name">The template name.</param>
/// <param name="Subject">The default-language subject, or <see langword="null"/> when that version does not exist.</param>
/// <param name="Enabled">Whether the template may be sent.</param>
/// <param name="DateModifiedUtc">When the default-language version was last saved, in UTC.</param>
public sealed record MessageTemplateSummary(int Id, string Name, string Subject, bool Enabled, DateTime? DateModifiedUtc);

/// <summary>The template's name, subject and switch; the message itself is edited on the design page.</summary>
public sealed class MessageTemplateDetails
{
    /// <summary>The template identifier. Zero inserts a template.</summary>
    public int Id { get; set; }

    /// <summary>The template name. It must be unique for the tenant.</summary>
    public string Name { get; set; }

    /// <summary>The default-language subject.</summary>
    public string Subject { get; set; }

    /// <summary>Whether the template may be sent.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Token names stored with the template.</summary>
    public List<string> Tokens { get; set; } = [];
}

/// <summary>The default-language document opened by the design page.</summary>
/// <param name="Id">The template identifier.</param>
/// <param name="Name">The template name.</param>
/// <param name="Subject">The default-language subject.</param>
/// <param name="Enabled">Whether the template may be sent.</param>
/// <param name="Body">A PageKrafter email document the editor can open.</param>
/// <param name="Tokens">Token names stored with the template.</param>
public sealed record MessageTemplateDocument(int Id, string Name, string Subject, bool Enabled, string Body, IReadOnlyList<string> Tokens);

/// <summary>
/// Admin operations for message templates. Templates are edited in the default language: sending
/// falls back to that version whenever no translation exists.
/// </summary>
public interface IMessageTemplateAdminService
{
    /// <summary>Lists the tenant's templates, ordered by name, with the default-language subject.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="search">Matches the template name. Whitespace lists every template.</param>
    /// <returns>The templates.</returns>
    Task<IReadOnlyList<MessageTemplateSummary>> ListAsync(int tenantId, string search);

    /// <summary>Loads a template's name, subject, enabled flag, and tokens.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="id">The template.</param>
    /// <returns>The details, or not found.</returns>
    Task<Result<MessageTemplateDetails>> GetDetailsAsync(int tenantId, int id);

    /// <summary>Loads the default-language document for the design page.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="id">The template.</param>
    /// <returns>The document, or not found. Raw HTML is wrapped so the editor can open it.</returns>
    Task<Result<MessageTemplateDocument>> GetDocumentAsync(int tenantId, int id);

    /// <summary>Inserts a template when <see cref="MessageTemplateDetails.Id"/> is zero, otherwise updates its name, subject, enabled flag, and tokens.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="details">The values to save. The name is required and must be unique for the tenant.</param>
    /// <returns>The saved details, invalid when the name or tokens are rejected, or not found when an existing template is missing.</returns>
    Task<Result<MessageTemplateDetails>> SaveDetailsAsync(int tenantId, MessageTemplateDetails details);

    /// <summary>Saves the default-language body, creating that version the first time.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="id">The template.</param>
    /// <param name="body">The stored body. <see langword="null"/> is stored as empty.</param>
    /// <returns>Success, or not found.</returns>
    Task<Result> SaveBodyAsync(int tenantId, int id, string body);

    /// <summary>Deletes the template and every language version.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="id">The template.</param>
    /// <returns>Success, or not found.</returns>
    Task<Result> DeleteAsync(int tenantId, int id);
}

/// <inheritdoc cref="IMessageTemplateAdminService" />
/// <param name="templateService">Loads and saves templates.</param>
/// <param name="versionService">Loads and saves the default-language version.</param>
public class MessageTemplateAdminService(
    IMessageTemplateService templateService,
    IMessageTemplateVersionService versionService) : IMessageTemplateAdminService
{
    /// <inheritdoc />
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

    /// <inheritdoc />
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
            Tokens = MessageTemplateTokens.Read(template.Tokens).ToList(),
        });
    }

    /// <inheritdoc />
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
            MessageTemplateDocuments.ForEditor(version?.Data),
            MessageTemplateTokens.Read(template.Tokens)));
    }

    /// <inheritdoc />
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

        var tokens = MessageTemplateTokens.Normalize(details.Tokens);
        if (!tokens.IsSuccess)
        {
            return Result<MessageTemplateDetails>.Invalid(tokens.ValidationErrors);
        }

        string tokenJson = MessageTemplateTokens.Write(tokens.Value);
        MessageTemplate template;
        if (details.Id == 0)
        {
            template = await templateService.InsertAsync(new MessageTemplate
            {
                TenantId = tenantId,
                Name = name,
                Enabled = details.Enabled,
                Tokens = tokenJson,
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
            template.Tokens = tokenJson;
            await templateService.UpdateAsync(template);
        }

        await SaveVersionAsync(template.Id, version => version.Subject = details.Subject?.Trim() ?? string.Empty);

        details.Id = template.Id;
        details.Name = name;
        details.Tokens = tokens.Value.ToList();
        return Result<MessageTemplateDetails>.Success(details);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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