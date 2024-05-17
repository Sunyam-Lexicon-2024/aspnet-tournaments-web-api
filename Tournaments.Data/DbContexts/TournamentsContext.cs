using Microsoft.EntityFrameworkCore;

namespace Tournaments.Data.DbContexts;

public class TournamentsContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tournament> Tournaments {get; set;} = null!;
    public DbSet<Game> Games { get; set; } = null!;
}