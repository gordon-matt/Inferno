using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.Messaging.Entities;

namespace Inferno.Web.Messaging.Services;

public interface IMessageTemplateVersionService : IGenericDataService<MessageTemplateVersion>
{
    MessageTemplateVersion FindOne(int messageTemplateId, string cultureCode);
}

public class MessageTemplateVersionService : GenericDataService<MessageTemplateVersion>, IMessageTemplateVersionService
{
    public MessageTemplateVersionService(ICacheManager cacheManager, IRepository<MessageTemplateVersion> repository)
        : base(cacheManager, repository)
    {
    }

    public MessageTemplateVersion FindOne(int messageTemplateId, string cultureCode)
    {
        using var connection = OpenConnection();
        var versions = connection.Query(x => x.MessageTemplateId == messageTemplateId).ToList();

        if (!string.IsNullOrEmpty(cultureCode))
        {
            var localized = versions.FirstOrDefault(x => x.CultureCode == cultureCode);
            if (localized != null)
            {
                return localized;
            }
        }

        return versions.FirstOrDefault(x => x.CultureCode == null);
    }
}
