using Duende.IdentityServer.Models;

namespace Idp.Swiyu.IdentityProvider;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("scope1"),
            new ApiScope("scope2"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "web-client",
                ClientSecrets = { new Secret("super-secret-$123".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:7187/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:7187/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:7187/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "scope2" }
            },
        };
}
