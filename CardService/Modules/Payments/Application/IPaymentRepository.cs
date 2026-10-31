using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Application;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<PaymentPage> GetByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
}

public sealed record PaymentPage(IReadOnlyList<Payment> Items, int TotalCount);
