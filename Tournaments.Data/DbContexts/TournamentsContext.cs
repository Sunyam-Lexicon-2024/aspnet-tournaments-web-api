namespace Tournaments.Data.DbContexts;

public class TournamentsContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Tournament> Tournaments => Set<Tournament>(); 
    public DbSet<Game> Games => Set<Game>();
}