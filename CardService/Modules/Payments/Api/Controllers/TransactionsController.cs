using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuyaCardFlow.Modules.Payments.Application;
using TuyaCardFlow.Modules.Payments.Application.UseCases;

namespace TuyaCardFlow.Modules.Payments.Api.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/transactions")]
public sealed class TransactionsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(TransactionHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TransactionHistoryResponse>> GetHistory(
        Guid userId, [FromServices] GetUserTransactions useCase, CancellationToken cancellationToken,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        CanAccess(userId) ? Ok(await useCase.ExecuteAsync(userId, page, pageSize, cancellationToken)) : NotFound();

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> GetById(
        Guid userId, Guid id, [FromServices] GetTransactionById useCase,
        CancellationToken cancellationToken) =>
        CanAccess(userId) ? Ok(await useCase.ExecuteAsync(userId, id, cancellationToken)) : NotFound();
    private bool CanAccess(Guid userId) => User.IsInRole("Administrator")
        || Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var current) && current == userId;

}
