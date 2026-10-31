using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Application;

public sealed record TransactionResponse(
    Guid Id, Guid UserId, Guid CardId, decimal Amount, string Currency,
    string? Description, DateTimeOffset OccurredAtUtc)
{
    internal static TransactionResponse From(Payment payment) => new(
        payment.Id, payment.UserId, payment.CardId, payment.Amount, payment.Currency,
        payment.Description, payment.OccurredAtUtc);
}
