using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ScreenTime.Application.Exceptions;

namespace ScreenTime.WebApi.Filters;

// Maps the exceptions the services throw onto HTTP status codes, so the
// endpoints answer with 400/403/404/409 instead of 500, and puts the exception
// message in ProblemDetails.Detail so the client has something to show.
//
// Applied per controller rather than as global middleware: account, auditoriums,
// bookings, movies and screenings opt in, while admin and auth still map their
// own outcomes and genres and seats are read-only with nothing to map.
public sealed class DomainExceptionFilterAttribute : ExceptionFilterAttribute
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
