namespace TuyaCardFlow.Modules.Payments.Application;

public interface IPaymentCardVerifier
{
    Task<bool> BelongsToUserAsync(Guid cardId, Guid userId, CancellationToken cancellationToken = default);
}
