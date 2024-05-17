namespace Tournaments.Core.Repositories;

public interface IUnitOfWork
{

    public IRepository<Tournament> TournamentRepository { get; }
    public IRepository<Game> GameRepository { get; }

    public Task CompleteAsync();
}