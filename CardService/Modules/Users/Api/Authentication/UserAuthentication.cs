using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Application.UseCases;

namespace TuyaCardFlow.Modules.Users.Api.Authentication;

public static class UserAuthentication
{
    public const string CookieName = "tuya.session";
    public const string VersionClaim = "tuya.auth_version";
    public const string CurrentUserItem = "tuya.current_user";
    public const string AdministratorRole = "Administrator";

    public static IServiceCollection AddUserAuthentication(
        this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var administrators = configuration.GetSection("UsersAuthentication:AdministratorUserIds").Get<Guid[]>() ?? [];
        services.AddSingleton(new UserAdministratorIds(administrators.ToHashSet()));
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
        {
            options.Cookie.Name = CookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context => WriteAuthenticationProblem(context.HttpContext, 401),
                OnRedirectToAccessDenied = context => WriteAuthenticationProblem(context.HttpContext, 403),
                OnValidatePrincipal = async context =>
                {
                    if (!Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                        || !Guid.TryParse(context.Principal?.FindFirstValue(VersionClaim), out var version))
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync();
                        return;
                    }
                    var validation = context.HttpContext.RequestServices.GetRequiredService<ValidateUserSession>();
                    var user = await validation.ExecuteAsync(id, version, context.HttpContext.RequestAborted);
                    var admin = context.HttpContext.RequestServices.GetRequiredService<UserAdministratorIds>().Values.Contains(id);
                    if (user is null || context.Principal!.IsInRole(AdministratorRole) != admin)
                    {
                        context.RejectPrincipal();
                        await context.HttpContext.SignOutAsync();
                        return;
                    }
                    context.HttpContext.Items[CurrentUserItem] = user;
                }
            };
        });
        services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser().Build());
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "tuya.csrf";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        });
        services.AddScoped<CookieCsrfFilter>();
        services.AddScoped<RequiredUserCsrfFilter>();
        services.Configure<MvcOptions>(options => options.Filters.AddService<CookieCsrfFilter>());
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("user-login", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 8, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.AddPolicy("user-registration", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = 429, Title = "Demasiados intentos", Detail = "Espera un minuto antes de volver a intentarlo."
                }, token);
            };
        });
        return services;
    }

    public static ClaimsPrincipal CreatePrincipal(UserLoginResult result, bool isAdministrator)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
            new(ClaimTypes.Name, result.User.FullName),
            new(ClaimTypes.Email, result.User.Email),
            new(VersionClaim, result.AuthenticationVersion.ToString())
        };
        if (isAdministrator)
            claims.Add(new Claim(ClaimTypes.Role, AdministratorRole));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    private static Task WriteAuthenticationProblem(HttpContext context, int status)
    {
        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = status == 401 ? "Inicia sesión para continuar" : "No tienes permiso para esta operación"
        });
    }
}

public sealed record UserAdministratorIds(IReadOnlySet<Guid> Values);

public sealed class CookieCsrfFilter(IAntiforgery antiforgery) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        if (!request.Cookies.ContainsKey(UserAuthentication.CookieName)
            || HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method))
            return;
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new BadRequestObjectResult(new ProblemDetails
            {
                Status = 400, Title = "No pudimos validar la solicitud", Detail = "Actualiza la página y vuelve a intentarlo."
            });
        }
    }
}

public sealed class RequiredUserCsrfFilter(IAntiforgery antiforgery) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new BadRequestObjectResult(new ProblemDetails
            {
                Status = 400, Title = "No pudimos validar la solicitud", Detail = "Actualiza la página y vuelve a intentarlo."
            });
        }
    }
}
