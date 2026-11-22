using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Inferno.Identity.Models.ManageViewModels;

/// <summary>The external logins linked to the account and the schemes still available.</summary>
public class ExternalLoginsViewModel
{
    /// <summary>External logins already linked to the account.</summary>
    public IList<UserLoginInfo> CurrentLogins { get; set; }

    /// <summary>Authentication schemes that are not yet linked.</summary>
    public IList<AuthenticationScheme> OtherLogins { get; set; }

    /// <summary>Whether a linked login can be removed. True when the account has a password or more than one login.</summary>
    public bool ShowRemoveButton { get; set; }

    /// <summary>A message from the previous manage action.</summary>
    public string StatusMessage { get; set; }
}