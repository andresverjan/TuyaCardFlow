using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Api.Errors;

public sealed class UsersExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not (UserValidationException or UserNotFoundException or DuplicateUserEmailException or InvalidUserCredentialsException))
            return false;

        var (status, title) = exception switch
        {
            UserValidationException => (StatusCodes.Status400BadRequest, "Datos de usuario inválidos"),
            InvalidUserCredentialsException => (StatusCodes.Status401Unauthorized, "No pudimos iniciar sesión"),
            DuplicateUserEmailException => (StatusCodes.Status409Conflict, "Correo ya registrado"),
            _ => (StatusCodes.Status404NotFound, "Usuario no encontrado")
        };
        httpContext.Response.StatusCode = status;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = status, Title = title, Detail = exception.Message, Instance = httpContext.Request.Path
            }
        });
        return true;
    }
}
