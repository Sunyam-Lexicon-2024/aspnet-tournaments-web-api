using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Tournaments.Data.Repositories;

public class TournamentRepository(
        TournamentsContext tournamentsContext) : IRepository<Tournament>
{
    private readonly TournamentsContext _tournamentsContext = tournamentsContext;

    public async Task<Tournament?> AddAsync(Tournament tournament)
    {
        var tournamentExists = await AnyAsync(t => t.Id == tournament.Id);
        if (tournamentExists) return null;
        var CreateAPIModelurnament = await _tournamentsContext.AddAsync(tournament);
        await _tournamentsContext.SaveChangesAsync();
        return CreateAPIModelurnament.Entity;
    }

    public async Task<bool> AnyAsync(Expression<Func<Tournament, bool>> predicate)
    {
        return await _tournamentsContext.Tournaments.AnyAsync(predicate);
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

            if (queryParameters.Filter is not null && queryParameters.Filter.Any())
            {
                tournaments = Filter(tournaments, queryParameters.Filter);
            }

            if (!string.IsNullOrEmpty(queryParameters.Sort))
            {
                // TBD Implement sorting direction bool
                tournaments = Sort(tournaments, queryParameters.Sort, true);
            }
            if (queryParameters.PageSize is not null)
            {
                if (queryParameters.LastId is not null)
                {
                    tournaments = tournaments
                    .Where(t => t.Id > queryParameters.LastId)
                    .Take((int)queryParameters.PageSize);
                }
                else if (queryParameters.CurrentPage is not null)
                {
                    int skipCount = (int)queryParameters.PageSize * (int)queryParameters.CurrentPage;
                    tournaments = tournaments
                    .Skip(skipCount)
                    .Take((int)queryParameters.PageSize);
                }
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
            var property = GetProperty(typeof(Tournament), filter.Key) ??
            throw new InvalidOperationException($"Could not determine property from filter key {filter.Key}");

            query = query
                         .Where(t => 
                         EF.Property<string>(t, property).Equals(filter.Value,
                            StringComparison.OrdinalIgnoreCase))
                         .Where(t => 
                         EF.Property<string>(t, property).Contains(filter.Value,
                            StringComparison.CurrentCultureIgnoreCase));
        }

        return query;
    }

    private static IQueryable<Tournament> Search(
        IQueryable<Tournament> query,
        string searchTerm)
    {
        query = query
                    .Where(t => t.Title.Contains(searchTerm, 
                        StringComparison.CurrentCultureIgnoreCase))
                    .Where(t => t.StartDate.ToString().Contains(searchTerm, 
                        StringComparison.CurrentCultureIgnoreCase));

        return query;
    }

    private static string? GetProperty(Type type, string propertyKey)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => string.Equals(
                p.Name,
                propertyKey,
                StringComparison.OrdinalIgnoreCase))?.Name;
    }
}