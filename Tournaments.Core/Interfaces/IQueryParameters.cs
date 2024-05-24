namespace Tournaments.Core.Interfaces;

public interface IQueryParameters
{
    public bool? IncludeGames { get; set; }
    public string? Title { get; set; }
    public string? Sort { get; set; }
    public IDictionary<string, string>? Filter { get; set; }
    public string? Search { get; set; }
    public int? PageSize { get; set; }
    public int? CurrentPage { get; set; }
    public int? LastId { get; set; }
}