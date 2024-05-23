using Games.Data.Repositories;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

namespace Tournaments.API.Extensions;

public static class WebAppExtensions
{
    public static async Task SeedDataAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var logger = app.ApplicationServices.GetService<ILogger<Program>>();
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

        services
            .AddSerilog()
            .AddDbContext<TournamentsContext>(opt =>
            opt.UseSqlServer(configuration.GetConnectionString("Default")))
            .AddScoped<IRepository<Tournament>, TournamentRepository>()
            .AddScoped<IRepository<Game>, GameRepository>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddLogging()
            .AddAutoMapper(config =>
        {
            config.AddProfile<TournamentMappingProfile>();
            config.AddProfile<GameMappingProfile>();
        });

        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Tournaments API",
                Description = "An ASP.NET Core Web API for managing Tournaments and their corresponding Games",
                Contact = new OpenApiContact
                {
                    Name = "Suny-Am",
                    Email = "visualarea.1@gmail.com",
                    Url = new Uri("https://github.com/suny-am")
                },
            });

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        return services;
    }
}