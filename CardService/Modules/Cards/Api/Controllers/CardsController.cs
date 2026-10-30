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
        Ok(await useCase.ExecuteAsync(userId, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponse>> GetById(
        Guid id, [FromServices] GetCardById useCase, CancellationToken cancellationToken) =>
        Ok(await useCase.ExecuteAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CardResponse>> Create(
        CreateCardRequest request, [FromServices] CreateCard useCase, CancellationToken cancellationToken)
    {
        var response = await useCase.ExecuteAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CardResponse>> Update(
        Guid id, UpdateCardRequest request, [FromServices] UpdateCard useCase,
        CancellationToken cancellationToken) =>
        Ok(await useCase.ExecuteAsync(id, request.ToCommand(), cancellationToken));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id, [FromServices] DeleteCard useCase, CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
