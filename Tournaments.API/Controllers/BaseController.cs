using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Tournaments.API.Controllers;

public abstract class BaseController(
    IUnitOfWork unitOfWork) : Controller
{
    protected readonly IUnitOfWork _unitOfWork = unitOfWork;

    protected virtual object ErrorResponseBody(ModelStateDictionary modelState)
    {
        var errorMessages = ModelState.Values
            .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            .ToList();

        return new
        {
            Message = "Parameter Validation Failed",
            Errors = errorMessages
        };
    }

    protected override void Dispose(bool disposing)
    {
        _unitOfWork.Dispose();
        base.Dispose(disposing);
    }

    protected virtual Dictionary<string, string> PaginationHeaders(QueryParameters queryParameters)
    {
        Dictionary<string, string> paginationHeaders = [];

        if (queryParameters.PageSize is not null)
        {
            paginationHeaders.Add("Page-Size", queryParameters.PageSize.ToString()!);
            if (queryParameters.CurrentPage is not null)
            {
                paginationHeaders.Add("Current-Page", queryParameters.CurrentPage.ToString()!);
            }
            else if (queryParameters.LastId is not null)
            {
                paginationHeaders.Add("Last-Id", queryParameters.LastId.ToString()!);
            }
        }
        return paginationHeaders;
    }
}