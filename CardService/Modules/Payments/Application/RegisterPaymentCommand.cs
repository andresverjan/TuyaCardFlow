namespace TuyaCardFlow.Modules.Payments.Application;

public sealed record RegisterPaymentCommand(
    Guid UserId, Guid CardId, decimal Amount, string Currency, string? Description);
