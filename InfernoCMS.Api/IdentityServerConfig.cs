using Duende.IdentityServer.Models;

namespace InfernoCMS.Api
{
    internal class IdentityServerConfig
    {
        internal static IEnumerable<IdentityResource> IdentityResources => new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

        internal static IEnumerable<ApiScope> ApiScopes => new ApiScope[]
        {
            new ApiScope("weatherapi.read"),
            new ApiScope("weatherapi.write"),
        };

        internal static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("weatherapi")
            {
                Scopes = new[] { "weatherapi.read", "weatherapi.write" },
                ApiSecrets = new[] { new Secret("ScopeSecret".Sha256()) },
                UserClaims = new[] { "role" }
            }
        };

        internal static IEnumerable<Client> Clients => new Client[]
        {
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                AllowedScopes = {"weatherapi.read", "weatherapi.write"}
            },

            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:5444/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:5444/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:5444/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "weatherapi.read" },
                RequireConsent = true
            },
        };
    }
}