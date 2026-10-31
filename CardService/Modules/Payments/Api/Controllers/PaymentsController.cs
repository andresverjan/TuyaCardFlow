using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TuyaCardFlow.Modules.Payments.Api.Contracts;
using TuyaCardFlow.Modules.Payments.Application;
using TuyaCardFlow.Modules.Payments.Application.UseCases;

namespace TuyaCardFlow.Modules.Payments.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponse>> Register(
        RegisterPaymentRequest request, [FromServices] RegisterPayment useCase,
        CancellationToken cancellationToken)
    {
        if (!CanAccess(request.UserId!.Value)) return NotFound();
        var transaction = await useCase.ExecuteAsync(request.ToCommand(), cancellationToken);
        return CreatedAtAction(nameof(TransactionsController.GetById), "Transactions",
            new { userId = transaction.UserId, id = transaction.Id }, transaction);
    }
    private bool CanAccess(Guid userId) => User.IsInRole("Administrator")
        || Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var current) && current == userId;

}
