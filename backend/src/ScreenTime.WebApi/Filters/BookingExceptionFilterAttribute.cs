using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ScreenTime.Application.Exceptions;

namespace ScreenTime.WebApi.Filters;

// Maps the exceptions BookingService throws onto HTTP status codes, so the
// booking endpoints answer with 404/403/409 instead of 500.
//
// Scoped to BookingsController on purpose: the other controllers have no error
// handling at all today, and changing their behaviour belongs in its own
// change. Lift this to global middleware when that happens.
public sealed class BookingExceptionFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var (statusCode, title) = context.Exception switch
        {
            SeatsUnavailableException =>
                (StatusCodes.Status409Conflict, "Sätena är inte tillgängliga"),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Hittades inte"),

            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden, "Åtkomst nekad"),

            ArgumentException =>
                (StatusCodes.Status400BadRequest, "Ogiltig begäran"),

            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "Åtgärden kan inte utföras"),

            // Not one of ours. Leave it unhandled rather than swallowing it.
            _ => (0, string.Empty)
        };

        if (statusCode == 0)
        {
            return;
        }

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = context.Exception.Message
        })
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}
