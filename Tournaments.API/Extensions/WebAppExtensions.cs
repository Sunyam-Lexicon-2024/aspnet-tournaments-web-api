using Games.Data.Repositories;

namespace Tournaments.API.Extensions;

public static class WebAppExtensions
{
    public static async Task SeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var logger = app.ApplicationServices.GetService<ILogger>();
        var serviceProvider = scope.ServiceProvider;
        var tournamentsContext = serviceProvider.GetRequiredService<TournamentsContext>();
        var unitOfWork = serviceProvider.GetService<IUnitOfWork>();

        SeedData seedData = new(logger!, unitOfWork!);

        await tournamentsContext.Database.EnsureDeletedAsync();
        await tournamentsContext.Database.MigrateAsync();

        try
        {
            await seedData.InitAsync();
        }
        catch (Exception ex)
        {
            logger!.LogError("{Message}", 
                JsonSerializer.Serialize(new { ex.Message, ex.StackTrace }));
            throw;
        }
    }

    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TournamentsContext>(opt =>
            opt.UseSqlServer(configuration.GetConnectionString("Default")));

        services.AddScoped<IRepository<Tournament>, TournamentRepository>();
        services.AddScoped<IRepository<Game>, GameRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddAutoMapper(config =>
        {
            config.AddProfile<TournamentMappingProfile>();
            config.AddProfile<GameMappingProfile>();
        });

        return services;
    }
}