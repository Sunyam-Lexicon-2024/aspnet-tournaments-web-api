namespace Tournaments.Data.Parameters;

public class QueryParameters : IQueryParameters
{
    public bool? IncludeChildren { get; set; }
    public string? Title { get; set; }
    public string? Sort { get; set; }
    public IDictionary<string, string>? Filter { get; set; }
    public string? Search { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}