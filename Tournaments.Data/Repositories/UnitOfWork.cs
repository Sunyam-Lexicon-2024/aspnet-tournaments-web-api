namespace Tournaments.Data.Repositories;

public class UnitOfWork(
    TournamentsContext tournamentsContext,
    IRepository<Tournament> tournamentRepository,
    IRepository<Game> gameRepository
) : IUnitOfWork
{
    private readonly TournamentsContext _tournamentsContext = tournamentsContext;
    private readonly IRepository<Tournament> _tournamentRepository = tournamentRepository;
    private readonly IRepository<Game> _gameRepository = gameRepository;

    public IRepository<Tournament> TournamentRepository => _tournamentRepository;

    public IRepository<Game> GameRepository => _gameRepository;

    public async Task CompleteAsync()
    {
        await _tournamentsContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose();
    }
}