using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Application.UseCases;

public sealed class GetTransactionById(IPaymentRepository repository)
{
    public async Task<TransactionResponse> ExecuteAsync(
        Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || id == Guid.Empty)
            throw new PaymentValidationException("El usuario y la transacción son obligatorios.");

        var payment = await repository.GetByIdForUserAsync(id, userId, cancellationToken)
            ?? throw new TransactionNotFoundException(id);

        return TransactionResponse.From(payment);
    }
}
