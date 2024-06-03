using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace IdentityService.Web;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
    [
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
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
            ClientId = "devClientInteractive",

            AllowedGrantTypes = GrantTypes.Code,
            AllowOfflineAccess =true,

            ClientSecrets =
            {
                new Secret("devSecretInteractive".Sha256())
            },

            AllowedScopes = {

                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile,
                IdentityServerConstants.StandardScopes.Email,

                "tournamentAPI"
            }
        },
    ];
}