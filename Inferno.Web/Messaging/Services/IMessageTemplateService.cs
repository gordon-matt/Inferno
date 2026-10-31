using System.Net.Mail;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.Messaging.Entities;

namespace Inferno.Web.Messaging.Services;

public interface IMessageTemplateService : IGenericDataService<MessageTemplate>
{
    MessageTemplate Find(int tenantId, string name);
}

public class MessageTemplateService : GenericDataService<MessageTemplate>, IMessageTemplateService
{
    public MessageTemplateService(ICacheManager cacheManager, IRepository<MessageTemplate> repository)
        : base(cacheManager, repository)
    {
    }

    public MessageTemplate Find(int tenantId, string name) => FindOne(new SearchOptions<MessageTemplate>
    {
        Query = x => x.TenantId == tenantId && x.Name == name
    });
}
