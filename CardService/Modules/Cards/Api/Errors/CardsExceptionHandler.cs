using TuyaCardFlow.Modules.Cards.Application;
using TuyaCardFlow.Modules.Cards.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TuyaCardFlow.Modules.Cards.Api.Errors;

public sealed class CardsExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not (CardValidationException or CardNotFoundException))
            return false;

        var (status, title) = exception switch
        {
            CardValidationException => (StatusCodes.Status400BadRequest,
                "Datos de tarjeta inválidos"),
            _ => (StatusCodes.Status404NotFound, "Tarjeta no encontrada")
        };

        httpContext.Response.StatusCode = status;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path
            }
        });

        return true;
    }
}
