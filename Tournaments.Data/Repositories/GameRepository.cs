using System.Linq.Expressions;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Games.Data.Repositories;

public class GameRepository(TournamentsContext tournamentsContext) : IRepository<Game>
{
    private readonly TournamentsContext _tournamentsContext = tournamentsContext;

    public async Task<Game?> AddAsync(Game tournament)
    {
        var tournamentExists = await AnyAsync(tournament.Id);
        if (tournamentExists) return null;
        var createdGame = await _tournamentsContext.AddAsync(tournament);
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
                .Where(t => t.Title.Equals(queryParameters.Title)).ToListAsync();
        }
        else
        {
            if (!string.IsNullOrEmpty(queryParameters.Search))
            {
                games = Search(games, queryParameters.Search);
            }

            if (queryParameters.Filter != null && queryParameters.Filter.Any())
            {
                games = Filter(games, queryParameters.Filter);
            }

            if (!string.IsNullOrEmpty(queryParameters.Sort))
            {
                // TBD Implement sorting direction bool
                games = Sort(games, queryParameters.Sort, true);
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

            query = query.Where(g =>
                EF.Property<string>(g, property) == filter.Value);
        }

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

    private static IQueryable<Game> Search(
        IQueryable<Game> query,
        string searchTerm)
    {
        var parameter = Expression.Parameter(typeof(Game), "e");
        var property = Expression.Property(parameter, "Title");
        var searchExpression = Expression.Constant(searchTerm);
        var containsMethod = typeof(string).GetMethod("Contains",
            [typeof(string)]);
        var containsExpression = Expression.Call(
            property,
            containsMethod!,
            searchExpression);
        var lambda = Expression.Lambda<Func<Game, bool>>(
            containsExpression,
            parameter);

        return query.Where(lambda);
    }
}