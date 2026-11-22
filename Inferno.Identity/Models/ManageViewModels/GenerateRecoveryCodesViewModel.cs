namespace Inferno.Identity.Models.ManageViewModels;

/// <summary>The recovery codes just generated for the account.</summary>
public class GenerateRecoveryCodesViewModel
{
    /// <summary>The new recovery codes.</summary>
    public string[] RecoveryCodes { get; set; }
}