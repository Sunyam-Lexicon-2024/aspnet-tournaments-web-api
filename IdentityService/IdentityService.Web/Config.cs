using Duende.IdentityServer.Models;

namespace IdentityService.Web;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId()
        };

    public static IEnumerable<ApiScope> ApiScopes =>
    new ApiScope[]
    {
        new ApiScope(name: "tournamentAPI", displayName: "Tournaments API")
    };

    public static IEnumerable<ApiResource> ApiResources =>
    new ApiResource[]
    {
        new ApiResource("tournamentAPI", "Tournaments API")
        {
            Scopes = { "tournamentAPI" }
        }
    };

    public static IEnumerable<Client> Clients =>
    new Client[]

    {
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
        }
    };
}