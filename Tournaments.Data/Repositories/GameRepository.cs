

using Microsoft.EntityFrameworkCore;
using Tournaments.Data.DbContexts;

namespace Games.Data.Repositories;

public class GameRepository(TournamentsContext tournamentsContext) : IRepository<Game>
{
    private readonly TournamentsContext _tournamentsContext = tournamentsContext;

    public async Task<Game?> AddAsync(Game tournament)
    {
        var tournamentExists = await AnyAsync(tournament.Id);
        if (tournamentExists) return null;
        var createdGame = await _tournamentsContext.AddAsync(tournament);
        await _tournamentsContext.SaveChangesAsync();
        return createdGame.Entity;
    }


    public async Task<bool> AnyAsync(int id)
    {
        return await _tournamentsContext.Games.AnyAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _tournamentsContext.Games.ToListAsync();
    }

    public async Task<Game?> GetAsync(int id)
    {
        return await _tournamentsContext.Games.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Game?> RemoveAsync(int gameId)
    {
        var gameToDelete = await _tournamentsContext.Games.FirstOrDefaultAsync(g => g.Id == gameId);
        if(gameToDelete is null) {
            return null;
        }
        var deletedGame = _tournamentsContext.Games.Remove(gameToDelete).Entity;
        await _tournamentsContext.SaveChangesAsync();
        return deletedGame;
    }

    public async Task<Game?> UpdateAsync(Game tournament)
    {
        var updatedGame = _tournamentsContext.Games.Update(tournament).Entity;
        await _tournamentsContext.SaveChangesAsync();
        return updatedGame;
    }
}