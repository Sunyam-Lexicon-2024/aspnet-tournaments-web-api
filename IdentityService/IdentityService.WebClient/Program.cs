using IdentityService.WebClient.Extensions;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorPages();

    builder.Services.RegisterApplicationServices(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        /*
        The default HSTS value is 30 days. 
        You may want to change this for production scenarios, 
        see https://aka.ms/aspnetcore-hsts.
        */
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting()
        .UseAuthentication()
        .UseAuthorization();

    app.MapRazorPages()
        .RequireAuthorization();

    await app.RunAsync();

    Log.Information("Application exited cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}