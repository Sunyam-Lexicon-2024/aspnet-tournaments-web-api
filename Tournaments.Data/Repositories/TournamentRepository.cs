using System.Linq.Expressions;
using System.Linq.Dynamic.Core;

namespace Tournaments.Data.Repositories;

public class TournamentRepository(
        TournamentsContext tournamentsContext) : IRepository<Tournament>
{
    private readonly TournamentsContext _tournamentsContext = tournamentsContext;

    public async Task<Tournament?> AddAsync(Tournament tournament)
    {
        var tournamentExists = await AnyAsync(tournament.Id);
        if (tournamentExists) return null;
        var CreateAPIModelurnament = await _tournamentsContext.AddAsync(tournament);
        await _tournamentsContext.SaveChangesAsync();
        return CreateAPIModelurnament.Entity;
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
        return await _tournamentsContext.Tournaments
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tournament?> GetAsyncWithChildren(int id)
    {
        return await _tournamentsContext.Tournaments
            .Include(t => t.Games)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Tournament>> GetAsyncByParams(
        IQueryParameters queryParameters)
    {
        var tournaments = _tournamentsContext.Tournaments.AsQueryable();

        if (!string.IsNullOrEmpty(queryParameters.Title))
        {
            return await tournaments
                .Where(t => t.Title.Equals(queryParameters.Title))
                .ToListAsync();
        }
        else
        {
            if (!string.IsNullOrEmpty(queryParameters.Search))
            {
                tournaments = Search(tournaments, queryParameters.Search);
            }

            if (queryParameters.Filter != null && queryParameters.Filter.Any())
            {
                tournaments = Filter(tournaments, queryParameters.Filter);
            }

            if (!string.IsNullOrEmpty(queryParameters.Sort))
            {
                // TBD Implement sorting direction bool
                tournaments = Sort(tournaments, queryParameters.Sort, true);
            }
        }
        return await tournaments.ToListAsync();
    }

    public async Task<Tournament?> RemoveAsync(int tournamentId)
    {
        var tournamentToDelete = await _tournamentsContext.Tournaments
            .FirstOrDefaultAsync(t => t.Id == tournamentId);
        if (tournamentToDelete is null)
        {
            return null;
        }
        var deletedTournament = _tournamentsContext.Tournaments
            .Remove(tournamentToDelete).Entity;
        return deletedTournament;
    }

    public async Task<Tournament?> UpdateAsync(Tournament tournament)
    {
        var updatedTournament = await Task.Run(() =>
        _tournamentsContext.Tournaments.Update(tournament).Entity);
        return updatedTournament;
    }

    private static IQueryable<Tournament> Sort(
        IQueryable<Tournament> query,
        string sortColumn,
        bool sortAscending)
    {
        string sortDirection = sortAscending ? "ascending" : "descending";
        return query.OrderBy($"{sortColumn} {sortDirection}");
    }

    private static IQueryable<Tournament> Filter(
        IQueryable<Tournament> query,
        IDictionary<string, string> filters)
    {
        foreach (var filter in filters)
        {
            query = query.Where(g =>
                EF.Property<string>(g, filter.Key) == filter.Value);
        }

        return query;
    }

    private static IQueryable<Tournament> Search(
        IQueryable<Tournament> query,
        string searchTerm)
    {
        var parameter = Expression.Parameter(typeof(Tournament), "e");
        var property = Expression.Property(parameter, "Title");
        var searchExpression = Expression.Constant(searchTerm);
        var containsMethod = typeof(string).GetMethod("Contains",
            [typeof(string)]);
        var containsExpression = Expression.Call(
            property,
            containsMethod!,
            searchExpression);
        var lambda = Expression.Lambda<Func<Tournament, bool>>(
            containsExpression,
            parameter);

        return query.Where(lambda);
    }
}