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

    protected void LogError(Exception exception, IBaseAPIModel apiModel, ILogger logger)
    {
        var errorDetails = JsonSerializer.Serialize(
            new
            {
                message = "Error processing item",
                model = apiModel,
                exception
            });
        logger.LogError("{Message}", errorDetails);
    }
    protected void LogError(Exception exception, IEnumerable<IBaseAPIModel> apiModels, ILogger logger)
    {
        var errorDetails = JsonSerializer.Serialize(
          new
          {
              message = "Error processing multiple items",
              models = apiModels,
              exception
          });
        logger.LogError("{Message}", errorDetails);
    }
    protected void LogError(Exception exception, int id, ILogger logger)
    {
        var errorDetails = JsonSerializer.Serialize(
           new
           {
               message = "Error processing ID",
               id,
               exception
           });
        logger.LogError("{Message}", errorDetails);
    }

    protected override void Dispose(bool disposing)
    {
        _unitOfWork.Dispose();
        base.Dispose(disposing);
    }
}