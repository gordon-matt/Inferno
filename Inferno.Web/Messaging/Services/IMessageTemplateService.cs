using System.Net.Mail;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.Messaging.Entities;

namespace Inferno.Web.Messaging.Services;

/// <summary>Loads message templates.</summary>
public interface IMessageTemplateService : IGenericDataService<MessageTemplate>
{
    /// <summary>The template named <paramref name="name"/> for <paramref name="tenantId"/>.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="name">The template name. The match is exact.</param>
    /// <returns>The template, or <see langword="null"/> when none is stored.</returns>
    MessageTemplate Find(int tenantId, string name);
}

/// <inheritdoc cref="IMessageTemplateService" />
public class MessageTemplateService : GenericDataService<MessageTemplate>, IMessageTemplateService
{
    /// <summary>Creates the service.</summary>
    /// <param name="cacheManager">Passed to the data service.</param>
    /// <param name="repository">The template table.</param>
    public MessageTemplateService(ICacheManager cacheManager, IRepository<MessageTemplate> repository)
        : base(cacheManager, repository)
    {
    }

    /// <inheritdoc />
    public MessageTemplate Find(int tenantId, string name) => FindOne(new SearchOptions<MessageTemplate>
    {
        Query = x => x.TenantId == tenantId && x.Name == name
    });
}
