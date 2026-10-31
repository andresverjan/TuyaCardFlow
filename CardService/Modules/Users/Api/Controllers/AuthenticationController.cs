using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TuyaCardFlow.Modules.Users.Api.Authentication;
using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Application.UseCases;

namespace TuyaCardFlow.Modules.Users.Api.Controllers;

[ApiController]
[Route("api/auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AuthenticationController : ControllerBase
{
    [HttpGet("csrf")]
    [AllowAnonymous]
    public IActionResult Csrf([FromServices] IAntiforgery antiforgery) =>
        Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });

    [HttpPost("login")]
    [AllowAnonymous]
    [ServiceFilter(typeof(RequiredUserCsrfFilter))]
    [EnableRateLimiting("user-login")]
    public async Task<ActionResult<UserResponse>> Login(LoginRequest request,
        [FromServices] LoginUser useCase, [FromServices] UserAdministratorIds administrators,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(new LoginUserCommand(request.Email, request.Password), cancellationToken);
        await HttpContext.SignInAsync(UserAuthentication.CreatePrincipal(result, administrators.Values.Contains(result.User.Id)),
            new AuthenticationProperties { IsPersistent = false, AllowRefresh = true });
        return Ok(result.User);
    }

    [HttpGet("me")]
    public ActionResult<UserResponse> Me() => HttpContext.Items[UserAuthentication.CurrentUserItem] is UserResponse user
        ? Ok(user) : Unauthorized();

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return NoContent();
    }
}

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128)]
    public string Password { get; init; } = string.Empty;
}
