

using Microsoft.EntityFrameworkCore;
using Tournaments.Data.DbContexts;

namespace Tournaments.Data.Repositories;

public class TournamentRepository(TournamentsContext tournamentsContext) : IRepository<Tournament>
{
    private readonly TournamentsContext _tournamentsContext = tournamentsContext;

    public async Task<Tournament?> AddAsync(Tournament tournament)
    {
        var tournamentExists = await AnyAsync(tournament.Id);
        if (tournamentExists) return null;
        var createdTournament = await _tournamentsContext.AddAsync(tournament);
        await _tournamentsContext.SaveChangesAsync();
        return createdTournament.Entity;
    }


    public async Task<bool> AnyAsync(int id)
    {
        return await _tournamentsContext.Tournaments.AnyAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Tournament>> GetAllAsync()
    {
        return await _tournamentsContext.Tournaments.ToListAsync();
    }

    public async Task<Tournament?> GetAsync(int id)
    {
        return await _tournamentsContext.Tournaments.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tournament?> RemoveAsync(int tournamentId)
    {
        var tournamentToDelete = await _tournamentsContext.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);
        if(tournamentToDelete is null) {
            return null;
        }
        var deletedTournament = _tournamentsContext.Tournaments.Remove(tournamentToDelete).Entity;
        await _tournamentsContext.SaveChangesAsync();
        return deletedTournament;
    }

    public async Task<Tournament?> UpdateAsync(Tournament tournament)
    {
        var updatedTournament = _tournamentsContext.Tournaments.Update(tournament).Entity;
        await _tournamentsContext.SaveChangesAsync();
        return updatedTournament;
    }
}