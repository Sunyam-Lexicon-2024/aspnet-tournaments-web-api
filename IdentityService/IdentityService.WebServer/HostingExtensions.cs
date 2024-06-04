using Serilog;
using Duende.IdentityServer;

namespace IdentityService.WebServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {

        // uncomment if you want to add a UI
        builder.Services.AddRazorPages();

        builder.Services.AddIdentityServer(options =>
            {
                // https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/api_scopes#authorization-based-on-scopes
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources)
            .AddInMemoryApiScopes(Config.ApiScopes)
            .AddInMemoryClients(Config.Clients)
            .AddTestUsers(TestUsers.Users);

        builder.Services.AddAuthentication()
                        .AddGoogle("Google", options =>
                        {
                            bool validAuthConfig = ValidateAuthConfiguration(builder.Configuration);

                            if (!validAuthConfig)
                            {
                                Log.Error("Invalid Auth Config");
                                return;
                            }

                            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                            options.CallbackPath = builder.Configuration["Authentication:Google:CallbackPath"];
                            options.CallbackPath = builder.Configuration["Authentication:Google:PostLogoutRedirectUri"];

                            options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // uncomment if you want to add a UI
        app.UseStaticFiles();
        app.UseRouting();

        app.UseIdentityServer();

        // uncomment if you want to add a UI
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        return app;
    }

    private static bool ValidateAuthConfiguration(IConfiguration configuration)
    {

        bool validAuthConfig = true;

        if (string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientId"]))
        {
            Log.Error("Could not load [Authentication:Google:ClientId] from configuration");
            validAuthConfig = false;
        }
        if (string.IsNullOrWhiteSpace(configuration["Authentication:Google:ClientSecret"]))
        {

            Log.Error("Could not load [Authentication:Google:ClientSecret] from configuration");
            validAuthConfig = false;
        }
        if (string.IsNullOrWhiteSpace(configuration["Authentication:Google:CallbackPath"]))
        {

            Log.Error("Could not load [Authentication:Google:CallbackPath] from configuration");
            validAuthConfig = false;
        }
        if (string.IsNullOrWhiteSpace(configuration["Authentication:Google:PostLogoutRedirectUri"]))
        {

            Log.Error("Could not load [Authentication:Google:PostLogoutRedirectUri] from configuration");
            validAuthConfig = false;
        }

        return validAuthConfig;

    }
}
