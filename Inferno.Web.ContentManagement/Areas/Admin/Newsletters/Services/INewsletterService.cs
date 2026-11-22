using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Events;
using Inferno.Web.Events;
using Inferno.Web.Security.Membership;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Services;

/// <summary>Signs a visitor up to the current tenant's newsletter.</summary>
public interface INewsletterService
{
    /// <summary>
    /// Subscribes <paramref name="email"/>. An existing account is updated only when it is <paramref name="currentUser"/>.
    /// A new account is created, and its generated password is included on the subscribed event.
    /// </summary>
    /// <param name="email">The address to subscribe. It is trimmed and must match <see cref="CmsConstants.RegexPatterns.Email"/>.</param>
    /// <param name="name">The subscriber's name. A name with a space is split into given names and a family name; otherwise it is stored as the given name.</param>
    /// <param name="currentUser">The signed-in user, or <see langword="null"/>. Only this user may subscribe an address that already has an account.</param>
    /// <returns>Whether the subscription was accepted, and a message to show.</returns>
    Task<NewsletterSubscriptionResult> SubscribeAsync(string email, string name, InfernoUser currentUser);
}

/// <inheritdoc cref="INewsletterService" />
public class NewsletterService : INewsletterService
{
    private readonly IMembershipService membershipService;
    private readonly MembershipSettings membershipSettings;
    private readonly IEventPublisher eventPublisher;
    private readonly IWorkContext workContext;
    private readonly IStringLocalizer localizer;

    /// <summary>Stores the membership, event, tenant, and localization services used to subscribe someone.</summary>
    /// <param name="membershipService">Finds, creates, and updates the subscriber's account.</param>
    /// <param name="membershipSettings">Supplies the length of a generated password.</param>
    /// <param name="eventPublisher">Publishes <see cref="NewsletterSubscribedEvent"/> after a successful subscription.</param>
    /// <param name="workContext">Supplies the current tenant.</param>
    /// <param name="localizer">Supplies the success and failure messages.</param>
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

    /// <inheritdoc />
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

/// <summary>The outcome of a newsletter subscription.</summary>
public sealed class NewsletterSubscriptionResult
{
    private NewsletterSubscriptionResult(bool succeeded, string message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    /// <summary><see langword="true"/> when the address was subscribed.</summary>
    public bool Succeeded { get; }

    /// <summary>The message to show the visitor.</summary>
    public string Message { get; }

    /// <summary>Creates a successful result.</summary>
    /// <param name="message">The message to show.</param>
    /// <returns>A result with <see cref="Succeeded"/> set.</returns>
    public static NewsletterSubscriptionResult Ok(string message) => new(true, message);

    /// <summary>Creates a failed result.</summary>
    /// <param name="message">The message to show.</param>
    /// <returns>A result with <see cref="Succeeded"/> clear.</returns>
    public static NewsletterSubscriptionResult Fail(string message) => new(false, message);
}
