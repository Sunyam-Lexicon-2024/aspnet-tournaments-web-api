using Microsoft.AspNetCore.Authentication;
using Serilog;

namespace IdentityService.WebClient.Extensions;

public static class WebAppExtensions
{
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        })
        .AddCookie("Cookies")
        // Auth code flow with PKCE
        // https://docs.duendesoftware.com/identityserver/v7/fundamentals/clients/
        .AddOpenIdConnect("oidc", options =>
        {

            bool validOIDC = ValidateOIDCConfig(configuration);

            if (!validOIDC)
            {
                Log.Error("Invalid OIDC Config");
                return;
            }

            options.Authority = configuration["Oidc:Authority"];

            options.CallbackPath = configuration["Oidc:CallbackPath"];
            options.SignedOutRedirectUri = configuration["Oidc:PostLogoutRedirectUri"]!;

            options.ClientId = "dev-web-client";
            options.ClientSecret = "dev-web-client-secret";
            options.ResponseType = "code";

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");

            options.ClaimActions.MapJsonKey("email_verified", "email_verified");

            options.GetClaimsFromUserInfoEndpoint = true;

            options.MapInboundClaims = false; // Disable claim relabling

            options.SaveTokens = true;
        });

        return services;
    }

    private static bool ValidateOIDCConfig(IConfiguration configuration)
    {
        bool validOIDCConfig = true;
        if (string.IsNullOrWhiteSpace(configuration["Oidc:Authority"]))
        {
            Log.Error("Could not load OIDC Authority from configuration");
            validOIDCConfig = false;
        }
        if (string.IsNullOrWhiteSpace(configuration["Oidc:CallbackPath"]))
        {

            Log.Error("Could not load OIDC Authority from configuration");
            validOIDCConfig = false;
        }
        if (string.IsNullOrWhiteSpace(configuration["Oidc:PostLogoutRedirectUri"]))
        {

            Log.Error("Could not load OIDC Authority from configuration");
            validOIDCConfig = false;
        }

        return validOIDCConfig;
    }
}