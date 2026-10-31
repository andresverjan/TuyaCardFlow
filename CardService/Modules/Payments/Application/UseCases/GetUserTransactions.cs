using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Application.UseCases;

public sealed class GetUserTransactions(IPaymentRepository repository)
{
    public async Task<TransactionHistoryResponse> ExecuteAsync(
        Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new PaymentValidationException("El usuario es obligatorio.");

        if (page < 1 || pageSize is < 1 or > 100 || (long)(page - 1) * pageSize > int.MaxValue)
            throw new PaymentValidationException("La página debe ser positiva y el tamaño debe estar entre 1 y 100.");

        var result = await repository.GetByUserAsync(userId, page, pageSize, cancellationToken);
        return new TransactionHistoryResponse(page, pageSize, result.TotalCount,
            result.Items.Select(TransactionResponse.From).ToArray());
    }
}
