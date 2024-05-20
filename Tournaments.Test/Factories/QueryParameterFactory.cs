namespace Tournaments.Test.Factories;

public class QueryParametersFactory
{

    public static QueryParameters IncludeChildrenParams()
    {
        return new QueryParameters()
        {
            IncludeChildren = true,
        };
    }

    public static QueryParameters TitleParams(string title)
    {
        return new QueryParameters()
        {
            Title = title
        };
    }

    public static QueryParameters SortParams(string sortColumn)
    {
        return new QueryParameters()
        {
            Sort = sortColumn
        };
    }

    public static QueryParameters FilterParams(IDictionary<string, string> filter)
    {
        return new QueryParameters()
        {
            Filter = filter
        };
    }

    public static QueryParameters SearchParams(string searchString)
    {
        return new QueryParameters()
        {
            Search = searchString
        };
    }

    public static QueryParameters PageParamsKeySet(int pageSize, int lastId)
    {
        return new QueryParameters()
        {
            PageSize = pageSize,
            LastId = lastId
        };
    }
    
    public static QueryParameters PageParamsOffset(int pageSize, int currentPage)
    {
        return new QueryParameters()
        {
            PageSize = pageSize,
            CurrentPage = currentPage
        };
    }
}