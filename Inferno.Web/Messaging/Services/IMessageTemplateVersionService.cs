using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.Messaging.Entities;

namespace Inferno.Web.Messaging.Services;

/// <summary>Loads the subject and body of a message template.</summary>
public interface IMessageTemplateVersionService : IGenericDataService<MessageTemplateVersion>
{
    /// <summary>
    /// The version for <paramref name="cultureCode"/>, or the version whose culture is <see langword="null"/> when that culture is missing or empty.
    /// Other cultures are not used as a fallback.
    /// </summary>
    /// <param name="messageTemplateId">The template.</param>
    /// <param name="cultureCode">The preferred culture. Empty or <see langword="null"/> uses the version with no culture.</param>
    /// <returns>The version, or <see langword="null"/> when neither match exists.</returns>
    MessageTemplateVersion FindOne(int messageTemplateId, string cultureCode);
}

/// <inheritdoc cref="IMessageTemplateVersionService" />
public class MessageTemplateVersionService : GenericDataService<MessageTemplateVersion>, IMessageTemplateVersionService
{
    /// <summary>Creates the service.</summary>
    /// <param name="cacheManager">Passed to the data service.</param>
    /// <param name="repository">The version table.</param>
    public MessageTemplateVersionService(ICacheManager cacheManager, IRepository<MessageTemplateVersion> repository)
        : base(cacheManager, repository)
    {
    }

    /// <inheritdoc />
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
