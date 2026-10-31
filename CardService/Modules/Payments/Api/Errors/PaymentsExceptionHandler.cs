using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TuyaCardFlow.Modules.Payments.Application;
using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Api.Errors;

public sealed class PaymentsExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not (PaymentValidationException or PaymentCardNotFoundException or TransactionNotFoundException))
            return false;

        var (status, title) = exception switch
        {
            PaymentValidationException => (StatusCodes.Status400BadRequest, "Datos de pago inválidos"),
            PaymentCardNotFoundException => (StatusCodes.Status404NotFound, "Tarjeta no encontrada"),
            _ => (StatusCodes.Status404NotFound, "Transacción no encontrada")
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
