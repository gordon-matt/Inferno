using Ardalis.Result;
using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Models;
using Inferno.Web.Events;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Services;

public interface ISubscriberAdminService
{
    /// <summary>Users of the tenant who have opted in to newsletters, matching <paramref name="search"/> when given.</summary>
    Task<Result<IReadOnlyList<Subscriber>>> ListAsync(int tenantId, string search);

    /// <summary>Opts the user out of newsletters. Their account is left alone.</summary>
    Task<Result> UnsubscribeAsync(int tenantId, string userId);
}

public class SubscriberAdminService(
    IMembershipService membershipService,
    IEventPublisher eventPublisher,
    ILogger<SubscriberAdminService> logger) : ISubscriberAdminService
{
    public async Task<Result<IReadOnlyList<Subscriber>>> ListAsync(int tenantId, string search)
    {
        try
        {
            var profileEntries = await membershipService.GetProfileEntriesByKeyAndValueAsync(
                tenantId,
                NewsletterUserProfileProvider.Fields.SubscribeToNewsletters,
                bool.TrueString);

            var userIds = profileEntries.Select(x => x.UserId).ToHashSet();
            if (userIds.Count == 0)
            {
                return Result.Success<IReadOnlyList<Subscriber>>([]);
            }

            var users = (await membershipService.GetAllUsersAsync(tenantId)).Where(x => userIds.Contains(x.Id)).ToList();
            var subscribers = new List<Subscriber>(users.Count);
            foreach (var user in users)
            {
                subscribers.Add(new Subscriber
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = await membershipService.GetUserDisplayNameAsync(user)
                });
            }

            IEnumerable<Subscriber> filtered = subscribers;
            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim();
                filtered = subscribers.Where(x =>
                    (x.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (x.Email?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            IReadOnlyList<Subscriber> ordered = filtered.OrderBy(x => x.Name).ThenBy(x => x.Email).ToList();
            return Result.Success(ordered);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not list newsletter subscribers for tenant {TenantId}", tenantId);
            return Result<IReadOnlyList<Subscriber>>.Error("The subscribers could not be loaded.");
        }
    }

    public async Task<Result> UnsubscribeAsync(int tenantId, string userId)
    {
        var user = string.IsNullOrEmpty(userId) ? null : await membershipService.GetUserByIdAsync(userId);
        if (user is null || user.TenantId != tenantId)
        {
            return Result.NotFound();
        }

        try
        {
            await membershipService.SaveProfileEntryAsync(userId, NewsletterUserProfileProvider.Fields.SubscribeToNewsletters, bool.FalseString);
            eventPublisher.Publish(new NewsletterUnsubscribedEvent(user));
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not unsubscribe user {UserId}", userId);
            return Result.Error("The subscriber could not be removed.");
        }
    }
}
