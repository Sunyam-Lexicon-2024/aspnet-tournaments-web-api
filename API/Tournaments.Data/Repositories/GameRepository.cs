using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Tournaments.Data.Parameters;

namespace Games.Data.Repositories;

public class GameRepository(TournamentsContext tournamentsContext) : IRepository<Game>
{
    private readonly TournamentsContext _tournamentsContext = tournamentsContext;

    public async Task<Game?> AddAsync(Game gameToAdd)
    {
        var tournamentExists = await AnyAsync(t => t.Id == gameToAdd.Id);
        if (tournamentExists) return null;
        var createdGame = await _tournamentsContext.AddAsync(gameToAdd);
        return createdGame.Entity;
    }

    public async Task<bool> AnyAsync(Expression<Func<Game, bool>> predicate)
    {
        return await _tournamentsContext.Games.AnyAsync(predicate);
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _tournamentsContext.Games.ToListAsync();
    }

    public async Task<Game?> GetAsync(int id)
    {
        return await _tournamentsContext.Games.FirstOrDefaultAsync(g => g.Id == id);
    }

    // stubbed method implementation for now
    public async Task<Game?> GetAsyncWithChildren(int id)
    {
        await Task.Run(() => id);
        return null;
    }

    public async Task<IEnumerable<Game>> GetAsyncByParams(
        IQueryParameters queryParameters)
    {
        var games = _tournamentsContext.Games.AsQueryable();

        if (!string.IsNullOrEmpty(queryParameters.Title))
        {
            return await games
                .Where(g => g.Title.Equals(queryParameters.Title)).ToListAsync();
        }
        else
        {
            if (!string.IsNullOrEmpty(queryParameters.Search))
            {
                games = Search(games, queryParameters.Search);
            }

            if (queryParameters.Filter is not null && queryParameters.Filter.Any())
            {
                games = Filter(games, queryParameters.Filter);
            }

            if (!string.IsNullOrEmpty(queryParameters.Sort))
            {
                // TBD Implement sorting direction bool
                games = Sort(games, queryParameters.Sort, true);
            }

            if (queryParameters.PageSize is not null)
            {
                if (queryParameters.LastId is not null)
                {
                    games = games
                    .Where(g => g.Id > queryParameters.LastId)
                    .Take((int)queryParameters.PageSize);
                }
                else if (queryParameters.CurrentPage is not null)
                {
                    int skipCount = (int)queryParameters.PageSize * (int)queryParameters.CurrentPage;
                    games = games
                    .Skip(skipCount)
                    .Take((int)queryParameters.PageSize);
                }
            }
        }
        return await games.ToListAsync();
    }

    public async Task<Game?> RemoveAsync(int gameId)
    {
        var gameToDelete = await _tournamentsContext.Games
            .FirstOrDefaultAsync(g => g.Id == gameId);
        if (gameToDelete is null)
        {
            return null;
        }
        var deletedGame = _tournamentsContext.Games.Remove(gameToDelete).Entity;
        return deletedGame;
    }

    public async Task<Game?> UpdateAsync(Game tournament)
    {
        var updatedGame = await Task.Run(() =>
            _tournamentsContext.Games.Update(tournament).Entity);
        return updatedGame;
    }

    private static IQueryable<Game> Sort(
        IQueryable<Game> query,
        string sortColumn,
        bool sortAscending)
    {
        string sortDirection = sortAscending ? "ascending" : "descending";
        return query.OrderBy($"{sortColumn} {sortDirection}");
    }

    private static IQueryable<Game> Filter(
        IQueryable<Game> query,
        IDictionary<string, string> filters)
    {
        foreach (var filter in filters)
        {
            var property = GetProperty(typeof(Game), filter.Key) ??
            throw new InvalidOperationException($"Could not determine property from filter key {filter.Key}");

            query = query
                .Where(g =>
                EF.Property<string>(g, property).Equals(filter.Value,
                    StringComparison.OrdinalIgnoreCase))
                .Where(g =>
                EF.Property<string>(g, property).Contains(filter.Value,
                    StringComparison.CurrentCultureIgnoreCase));
        }

        return query;
    }

    private static IQueryable<Game> Search(
        IQueryable<Game> query,
        string searchTerm)
    {
        query = query
                    .Where(g => g.TournamentId.ToString().Contains(searchTerm))
                    .Where(g => g.Title.Contains(searchTerm))
                    .Where(g => g.Id.ToString().Contains(searchTerm));

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