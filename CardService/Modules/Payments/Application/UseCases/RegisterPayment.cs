using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Application.UseCases;

public sealed class RegisterPayment(
    IPaymentRepository repository, IPaymentCardVerifier cardVerifier, TimeProvider timeProvider)
{
    public async Task<TransactionResponse> ExecuteAsync(
        RegisterPaymentCommand command, CancellationToken cancellationToken = default)
    {
        var payment = Payment.Register(command.UserId, command.CardId, command.Amount,
            command.Currency, command.Description, timeProvider.GetUtcNow());

        if (!await cardVerifier.BelongsToUserAsync(command.CardId, command.UserId, cancellationToken))
            throw new PaymentCardNotFoundException(command.CardId);

        await repository.AddAsync(payment, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return TransactionResponse.From(payment);
    }
}
