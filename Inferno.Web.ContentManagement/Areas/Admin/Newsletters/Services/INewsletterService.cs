using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;
using Inferno.Web.Events;
using Inferno.Web.Security.Membership;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Services;

public interface INewsletterService
{
    Task<NewsletterSubscriptionResult> SubscribeAsync(string email, string name, InfernoUser currentUser);
}

public class NewsletterService : INewsletterService
{
    private readonly IMembershipService membershipService;
    private readonly MembershipSettings membershipSettings;
    private readonly IEventPublisher eventPublisher;
    private readonly IWorkContext workContext;
    private readonly IStringLocalizer localizer;

    public NewsletterService(
        IMembershipService membershipService,
        MembershipSettings membershipSettings,
        IEventPublisher eventPublisher,
        IWorkContext workContext,
        IStringLocalizer localizer)
    {
        this.membershipService = membershipService;
        this.membershipSettings = membershipSettings;
        this.eventPublisher = eventPublisher;
        this.workContext = workContext;
        this.localizer = localizer;
    }

    public async Task<NewsletterSubscriptionResult> SubscribeAsync(string email, string name, InfernoUser currentUser)
    {
        email = email?.Trim() ?? string.Empty;
        name = name?.Trim() ?? string.Empty;

        if (!CmsConstants.RegexPatterns.Email.IsMatch(email))
        {
            return NewsletterSubscriptionResult.Fail(localizer[InfernoWebLocalizableStrings.Membership.InvalidEmailAddress]);
        }

        int tenantId = workContext.CurrentTenant.Id;
        var existingUser = await membershipService.GetUserByEmailAsync(tenantId, email);

        if (existingUser != null)
        {
            if (currentUser != null && currentUser.Id == existingUser.Id)
            {
                await membershipService.SaveProfileEntryAsync(
                    currentUser.Id,
                    NewsletterUserProfileProvider.Fields.SubscribeToNewsletters,
                    bool.TrueString);

                eventPublisher.Publish(new NewsletterSubscribedEvent(existingUser));
                return NewsletterSubscriptionResult.Ok(localizer[InfernoCmsLocalizableStrings.Newsletters.SuccessfullySignedUp]);
            }

            return NewsletterSubscriptionResult.Fail(localizer[InfernoWebLocalizableStrings.Membership.UserEmailAlreadyExists]);
        }

        string password = Password.Generate(
            membershipSettings.GeneratedPasswordLength,
            membershipSettings.GeneratedPasswordNumberOfNonAlphanumericChars);

        await membershipService.InsertUserAsync(new InfernoUser
        {
            TenantId = tenantId,
            UserName = email,
            Email = email
        }, password);

        var user = await membershipService.GetUserByEmailAsync(tenantId, email);
        if (user == null)
        {
            return NewsletterSubscriptionResult.Fail(localizer[InfernoWebLocalizableStrings.Membership.InsertUserError]);
        }

        await membershipService.SaveProfileEntryAsync(user.Id, NewsletterUserProfileProvider.Fields.SubscribeToNewsletters, bool.TrueString);
        await SaveNameAsync(user.Id, name);

        eventPublisher.Publish(new NewsletterSubscribedEvent(user, password));
        return NewsletterSubscriptionResult.Ok(localizer[InfernoCmsLocalizableStrings.Newsletters.SuccessfullySignedUp]);
    }

    private async Task SaveNameAsync(string userId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        if (name.Contains(' '))
        {
            string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string familyName = parts[^1];
            string givenNames = string.Join(' ', parts[..^1]);
            await membershipService.SaveProfileEntryAsync(userId, AccountUserProfileProvider.Fields.FamilyName, familyName);
            await membershipService.SaveProfileEntryAsync(userId, AccountUserProfileProvider.Fields.GivenNames, givenNames);
        }
        else
        {
            await membershipService.SaveProfileEntryAsync(userId, AccountUserProfileProvider.Fields.GivenNames, name);
        }
    }
}

public sealed class NewsletterSubscriptionResult
{
    private NewsletterSubscriptionResult(bool succeeded, string message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public bool Succeeded { get; }

    public string Message { get; }

    public static NewsletterSubscriptionResult Ok(string message) => new(true, message);

    public static NewsletterSubscriptionResult Fail(string message) => new(false, message);
}
