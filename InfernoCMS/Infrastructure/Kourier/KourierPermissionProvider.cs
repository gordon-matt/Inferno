using Inferno.Web.Security.Permissions;
using Kourier.Core;

namespace InfernoCMS.Infrastructure.Kourier;

/// <summary>Declares Kourier's policies as Inferno permissions so they can be granted per role.</summary>
public sealed class KourierPermissionProvider : IPermissionProvider
{
    private const string Email = "Email";

    /// <inheritdoc />
    public IEnumerable<Permission> GetPermissions() =>
    [
        new(KourierPolicies.View, Email, "View email admin"),
        new(KourierPolicies.ManageTemplates, Email, "Manage email templates") { Implies = [KourierPolicies.View] },
        new(KourierPolicies.SendTest, Email, "Send test emails") { Implies = [KourierPolicies.View] },
        new(KourierPolicies.ManageQueue, Email, "Manage the email queue") { Implies = [KourierPolicies.View] },
        new(KourierPolicies.ManageAccounts, Email, "Manage delivery accounts") { Implies = [KourierPolicies.View] },
        new(KourierPolicies.ManageSuppressions, Email, "Manage suppressions") { Implies = [KourierPolicies.View] },
        new(KourierPolicies.ManageContacts, Email, "Manage contacts and lists") { Implies = [KourierPolicies.View] },
        new(KourierPolicies.ManageCampaigns, Email, "Manage campaigns") { Implies = [KourierPolicies.View] },
        new(KourierPolicies.SendCampaigns, Email, "Send campaigns") { Implies = [KourierPolicies.ManageCampaigns] },
        new(KourierPolicies.ManageSettings, Email, "Change email settings") { Implies = [KourierPolicies.View] },
    ];
}