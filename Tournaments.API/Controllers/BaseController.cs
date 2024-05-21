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

    protected override void Dispose(bool disposing) {
        _unitOfWork.Dispose();
        base.Dispose(disposing);
    }
}