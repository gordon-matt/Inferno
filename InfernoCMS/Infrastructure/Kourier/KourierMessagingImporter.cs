using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using Inferno.Web.Configuration.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Messaging;
using Inferno.Web.Messaging;
using Inferno.Web.Messaging.Configuration;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Messaging.Services;
using Kourier.Core;
using Kourier.Data;
using Kourier.Data.Entities;
using Kourier.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace InfernoCMS.Infrastructure.Kourier;

/// <summary>
/// One-time cutover: copies Inferno message templates and SMTP settings into Kourier, then drains any
/// remaining queued emails into Kourier's queue. Safe to run on every start; it skips work already done.
/// </summary>
public sealed partial class KourierMessagingImporter(
    IServiceScopeFactory scopes,
    ILogger<KourierMessagingImporter> logger) : IHostedService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var services = scope.ServiceProvider;
            var factory = services.GetRequiredService<IKourierDbContextFactory>();
            var protector = services.GetRequiredService<KourierProtector>();
            var tenants = services.GetRequiredService<ITenantService>();
            var settings = services.GetRequiredService<ISettingService>();
            var templates = services.GetRequiredService<IRepository<MessageTemplate>>();
            var versions = services.GetRequiredService<IRepository<MessageTemplateVersion>>();
            var queued = services.GetRequiredService<IRepository<QueuedEmail>>();
            var email = services.GetRequiredService<IKourierEmailService>();

            var tenantList = (await tenants.FindAsync(new SearchOptions<Tenant> { PageNumber = 1, PageSize = int.MaxValue })).ToList();
            if (tenantList.Count == 0)
            {
                await ImportForTenantAsync(factory, protector, settings, templates, versions, queued, email, tenantId: null, cancellationToken);
                return;
            }

            foreach (var tenant in tenantList)
            {
                await ImportForTenantAsync(factory, protector, settings, templates, versions, queued, email, tenant.Id, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kourier messaging import failed. Email sending may still work for new mail.");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task ImportForTenantAsync(
        IKourierDbContextFactory factory,
        KourierProtector protector,
        ISettingService settings,
        IRepository<MessageTemplate> templates,
        IRepository<MessageTemplateVersion> versions,
        IRepository<QueuedEmail> queued,
        IKourierEmailService email,
        int? tenantId,
        CancellationToken cancellationToken)
    {
        string kourierTenant = tenantId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        await ImportSmtpAsync(factory, protector, settings, tenantId, kourierTenant, cancellationToken);
        await ImportTemplatesAsync(factory, templates, versions, tenantId, kourierTenant, cancellationToken);
        await DrainQueueAsync(queued, email, tenantId, kourierTenant, cancellationToken);
    }

    private async Task ImportSmtpAsync(
        IKourierDbContextFactory factory,
        KourierProtector protector,
        ISettingService settings,
        int? tenantId,
        string kourierTenant,
        CancellationToken cancellationToken)
    {
        var smtp = settings.GetSettings<SmtpSettings>(tenantId);
        if (string.IsNullOrWhiteSpace(smtp.Host))
        {
            return;
        }

        await using var context = (KourierDbContextBase)factory.GetContext();
        const string key = "smtp";
        var existing = await context.SenderAccounts
            .FirstOrDefaultAsync(account => account.TenantId == kourierTenant && account.Key == key, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        string fromAddress = DefaultEmailSender.IsValidEmailAddress(smtp.Username)
            ? smtp.Username.Trim()
            : "noreply@localhost";

        var plain = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase)
        {
            ["host"] = smtp.Host.Trim(),
            ["port"] = smtp.Port.ToString(CultureInfo.InvariantCulture),
            ["security"] = smtp.EnableSsl ? "StartTls" : "Auto",
            ["username"] = smtp.UseDefaultCredentials ? null : smtp.Username,
        };

        var secrets = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        if (!smtp.UseDefaultCredentials && !string.IsNullOrEmpty(smtp.Password))
        {
            secrets["password"] = smtp.Password;
        }

        var now = DateTime.UtcNow;
        context.SenderAccounts.Add(new SenderAccount
        {
            TenantId = kourierTenant,
            Key = key,
            Name = string.IsNullOrWhiteSpace(smtp.DisplayName) ? "SMTP" : smtp.DisplayName.Trim(),
            ProviderType = Constants.ProviderTypes.Smtp,
            SettingsJson = JsonSerializer.Serialize(plain, Json),
            ProtectedSecrets = secrets.Count == 0 ? null : protector.ProtectSecret(JsonSerializer.Serialize(secrets, Json)),
            FromAddress = fromAddress,
            FromName = smtp.DisplayName,
            IsEnabled = true,
            IsDefault = true,
            FromConfiguration = false,
            CreatedUtc = now,
            UpdatedUtc = now,
        });
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Imported SMTP settings for tenant {TenantId} as Kourier account 'smtp'", kourierTenant);
    }

    private async Task ImportTemplatesAsync(
        IKourierDbContextFactory factory,
        IRepository<MessageTemplate> templates,
        IRepository<MessageTemplateVersion> versions,
        int? tenantId,
        string kourierTenant,
        CancellationToken cancellationToken)
    {
        var source = (await templates.FindAsync(new SearchOptions<MessageTemplate>
        {
            Query = template => template.TenantId == tenantId,
            PageNumber = 1,
            PageSize = int.MaxValue,
        })).ToList();

        if (source.Count == 0)
        {
            return;
        }

        await using var context = (KourierDbContextBase)factory.GetContext();
        var existingKeys = await context.Templates
            .Where(template => template.TenantId == kourierTenant)
            .Select(template => template.Key)
            .ToListAsync(cancellationToken);
        var existing = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var template in source)
        {
            string key = ClipKey(template.Name);
            if (existing.Contains(key))
            {
                continue;
            }

            var contentVersions = (await versions.FindAsync(new SearchOptions<MessageTemplateVersion>
            {
                Query = version => version.MessageTemplateId == template.Id,
                PageNumber = 1,
                PageSize = int.MaxValue,
            })).ToList();

            if (contentVersions.Count == 0)
            {
                continue;
            }

            string bodyFormat = contentVersions.Any(version => MessageTemplateDocuments.IsDocument(version.Data))
                ? "pagekrafter"
                : Constants.BodyFormats.Html;

            var now = DateTime.UtcNow;
            var row = new Template
            {
                TenantId = kourierTenant,
                Key = key,
                Name = string.IsNullOrWhiteSpace(template.Name) ? key : template.Name.Trim(),
                Category = "Imported",
                BodyFormat = bodyFormat,
                MessageCategory = Constants.Categories.Transactional,
                IsEnabled = template.Enabled,
                IsSystem = false,
                Revision = 1,
                CreatedUtc = now,
                UpdatedUtc = now,
            };

            foreach (var version in contentVersions)
            {
                string culture = version.CultureCode?.Trim() ?? string.Empty;
                string body = ConvertTokens(version.Data ?? string.Empty);
                string subject = ConvertTokens(version.Subject ?? string.Empty);
                row.Contents.Add(new TemplateContent
                {
                    Culture = culture,
                    Subject = subject.Length > Constants.MaxLengths.Subject ? subject[..Constants.MaxLengths.Subject] : subject,
                    BodySource = body,
                });
            }

            context.Templates.Add(row);
            existing.Add(key);
            logger.LogInformation("Imported message template {TemplateName} for tenant {TenantId}", template.Name, kourierTenant);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task DrainQueueAsync(
        IRepository<QueuedEmail> queued,
        IKourierEmailService email,
        int? tenantId,
        string kourierTenant,
        CancellationToken cancellationToken)
    {
        var pending = (await queued.FindAsync(new SearchOptions<QueuedEmail>
        {
            Query = item => item.TenantId == tenantId && item.SentOnUtc == null,
            PageNumber = 1,
            PageSize = int.MaxValue,
        })).OrderBy(item => item.Priority).ThenBy(item => item.CreatedOnUtc).ToList();

        if (pending.Count == 0)
        {
            return;
        }

        int moved = 0;
        foreach (var item in pending)
        {
            try
            {
                var wrapper = MailMessageWrapper.FromJson(item.MailMessage);
                string to = !string.IsNullOrWhiteSpace(item.ToAddress)
                    ? item.ToAddress
                    : wrapper.To.FirstOrDefault() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(to))
                {
                    item.SentTries++;
                    await queued.UpdateAsync(item);
                    continue;
                }

                var result = await email.SendAsync(
                    new KourierEmail
                    {
                        To = KourierRecipient.Email(to, item.ToName),
                        Subject = item.Subject ?? wrapper.Subject ?? string.Empty,
                        HtmlBody = wrapper.IsBodyHtml ? wrapper.Body : null,
                        TextBody = wrapper.IsBodyHtml ? null : wrapper.Body,
                    },
                    new KourierSendOptions
                    {
                        TenantId = kourierTenant,
                        From = string.IsNullOrWhiteSpace(item.FromAddress)
                            ? null
                            : new KourierAddress(item.FromAddress, item.FromName),
                        IdempotencyKey = $"inferno-queued:{item.Id:N}",
                    },
                    cancellationToken);

                if (result.IsSuccess)
                {
                    item.SentOnUtc = DateTime.UtcNow;
                    await queued.UpdateAsync(item);
                    moved++;
                }
                else
                {
                    item.SentTries++;
                    await queued.UpdateAsync(item);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not drain queued email {QueuedEmailId}", item.Id);
                item.SentTries++;
                await queued.UpdateAsync(item);
            }
        }

        if (moved > 0)
        {
            logger.LogInformation("Moved {Count} queued emails into Kourier for tenant {TenantId}", moved, kourierTenant);
        }
    }

    private static string ClipKey(string? name)
    {
        string key = (name ?? "template").Trim();
        return key.Length <= Constants.MaxLengths.Key ? key : key[..Constants.MaxLengths.Key];
    }

    /// <summary>Rewrites Inferno's PascalCase Liquid tokens to the snake_case names Kourier's model dictionaries use.</summary>
    private static string ConvertTokens(string source) =>
        TokenRegex().Replace(source, match => "{{ " + ToSnakeCase(match.Groups[1].Value) + " }}");

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var builder = new System.Text.StringBuilder(name.Length + 8);
        for (int index = 0; index < name.Length; index++)
        {
            char current = name[index];
            if (char.IsUpper(current))
            {
                bool previousIsLowerOrDigit = index > 0 && (char.IsLower(name[index - 1]) || char.IsDigit(name[index - 1]));
                bool nextIsLower = index + 1 < name.Length && char.IsLower(name[index + 1]);
                if (index > 0 && (previousIsLowerOrDigit || nextIsLower))
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(current));
            }
            else
            {
                builder.Append(current);
            }
        }

        return builder.ToString();
    }

    [GeneratedRegex(@"\{\{\s*([A-Za-z_][A-Za-z0-9_]*)\s*\}\}", RegexOptions.Compiled)]
    private static partial Regex TokenRegex();
}