using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityService.Web;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResource() {
            Name = "verification",
            UserClaims = new List<string>
            {
                JwtClaimTypes.Email,
                JwtClaimTypes.EmailVerified,
            }
        }
    ];

    public static IEnumerable<ApiScope> ApiScopes =>
    [
        new ApiScope(name: "tournamentAPI", displayName: "Tournaments API")
    ];

    public static IEnumerable<ApiResource> ApiResources =>
    [
        new ApiResource("tournamentAPI", "Tournaments API")
        {
            Scopes = { "tournamentAPI" }
        }
    ];

    public static IEnumerable<Client> Clients =>
    [
        new Client
        {
            ClientId = "devClient",

            // no interactive user, use the clientid/secret for authentication
            AllowedGrantTypes = GrantTypes.ClientCredentials,

            // secret for authentication
            ClientSecrets =
            {
                new Secret("devSecret".Sha256())
            },

            // scopes that client has access to
            AllowedScopes = { "tournamentAPI" }
        },
        new Client
        {
            ClientId = "devWeb",

            AllowedGrantTypes = GrantTypes.Code,

            ClientSecrets =
            {
                new Secret("devWebSecret".Sha256())
            },

            RedirectUris = { "https://localhost:5002/signin-oidc" },

            PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },

            AllowedScopes = {

                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
            }
        },
    ];
}