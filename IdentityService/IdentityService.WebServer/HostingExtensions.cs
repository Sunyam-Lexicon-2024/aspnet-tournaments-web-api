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
                            options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                            if (string.IsNullOrWhiteSpace(builder.Configuration["Authentication:Google:ClientId"]))
                            {
                                Log.Error("Could not find Google Oauth Client ID In Configuration");
                                return;
                            }
                            if (string.IsNullOrWhiteSpace(builder.Configuration["Authentication:Google:ClientId"]))
                            {
                                Log.Error("Could not find Google Oauth Client Secret In Configuration");
                                return;
                            }

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
}
