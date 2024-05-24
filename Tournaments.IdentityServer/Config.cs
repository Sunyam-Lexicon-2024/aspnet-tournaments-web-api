using Duende.IdentityServer.Models;

namespace Tournaments.IdentityServer;

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