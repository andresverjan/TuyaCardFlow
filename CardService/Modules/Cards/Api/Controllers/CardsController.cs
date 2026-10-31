using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TuyaCardFlow.Modules.Cards.Api.Contracts;
using TuyaCardFlow.Modules.Cards.Application;
using TuyaCardFlow.Modules.Cards.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace TuyaCardFlow.Modules.Cards.Api.Controllers;

[ApiController]
[Route("api/cards")]
public sealed class CardsController : ControllerBase
{
    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CardResponse>>> GetUserCards(
        Guid userId, [FromServices] GetUserCards useCase, CancellationToken cancellationToken) =>
        CanAccess(userId) ? Ok(await useCase.ExecuteAsync(userId, cancellationToken)) : NotFound();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponse>> GetById(
        Guid id, [FromServices] GetCardById useCase, CancellationToken cancellationToken)
    {
        var card = await useCase.ExecuteAsync(id, cancellationToken);
        return CanAccess(card.UserId) ? Ok(card) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CardResponse>> Create(
        CreateCardRequest request, [FromServices] CreateCard useCase, CancellationToken cancellationToken)
    {
        if (!CanAccess(request.UserId!.Value)) return NotFound();
        var response = await useCase.ExecuteAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponse>> Update(
        Guid id, UpdateCardRequest request, [FromServices] UpdateCard useCase,
        [FromServices] GetCardById getCard, CancellationToken cancellationToken)
    {
        if (!CanAccess((await getCard.ExecuteAsync(id, cancellationToken)).UserId)) return NotFound();
        return Ok(await useCase.ExecuteAsync(id, request.ToCommand(), cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id, [FromServices] DeleteCard useCase, [FromServices] GetCardById getCard, CancellationToken cancellationToken)
    {
        if (!CanAccess((await getCard.ExecuteAsync(id, cancellationToken)).UserId)) return NotFound();
        await useCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
    private bool CanAccess(Guid userId) => User.IsInRole("Administrator")
        || Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var current) && current == userId;

}
