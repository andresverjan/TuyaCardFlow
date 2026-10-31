using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TuyaCardFlow.Modules.Users.Api.Authentication;
using TuyaCardFlow.Modules.Users.Api.Contracts;
using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Application.UseCases;

namespace TuyaCardFlow.Modules.Users.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [ServiceFilter(typeof(RequiredUserCsrfFilter))]
    [EnableRateLimiting("user-registration")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(
        CreateUserRequest request, [FromServices] CreateUser useCase, CancellationToken cancellationToken)
    {
        var user = await useCase.ExecuteAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserListResponse>> GetAll(
        [FromServices] GetUsers useCase, CancellationToken cancellationToken,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool? isActive = null) =>
        Ok(await useCase.ExecuteAsync(page, pageSize, isActive, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(
        Guid id, [FromServices] GetUserById useCase, CancellationToken cancellationToken)
    {
        if (!CanAccess(id)) return NotFound();
        return Ok(await useCase.ExecuteAsync(id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Update(
        Guid id, UpdateUserRequest request, [FromServices] UpdateUser useCase,
        CancellationToken cancellationToken)
    {
        if (!CanAccess(id)) return NotFound();
        return Ok(await useCase.ExecuteAsync(id, request.ToCommand(), cancellationToken));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> SetStatus(
        Guid id, SetUserStatusRequest request, [FromServices] SetUserStatus useCase,
        CancellationToken cancellationToken) =>
        Ok(await useCase.ExecuteAsync(id, request.IsActive!.Value, cancellationToken));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(
        Guid id, [FromServices] SetUserStatus useCase, CancellationToken cancellationToken)
    {
        if (!CanAccess(id)) return NotFound();
        await useCase.ExecuteAsync(id, false, cancellationToken);
        return NoContent();
    }
    private bool CanAccess(Guid userId) => User.IsInRole("Administrator")
        || Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var current) && current == userId;

}
