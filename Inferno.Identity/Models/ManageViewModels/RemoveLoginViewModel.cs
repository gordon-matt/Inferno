namespace Inferno.Identity.Models.ManageViewModels;

/// <summary>Identifies one external login to unlink from the account.</summary>
public class RemoveLoginViewModel
{
    /// <summary>The authentication scheme name.</summary>
    public string LoginProvider { get; set; }

    /// <summary>The provider's user key.</summary>
    public string ProviderKey { get; set; }
}