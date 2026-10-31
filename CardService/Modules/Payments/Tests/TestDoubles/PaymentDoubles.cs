using TuyaCardFlow.Modules.Payments.Application;
using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Tests.TestDoubles;

internal sealed class StubPaymentRepository : IPaymentRepository
{
    public List<Payment> Added { get; } = [];
    public List<Payment> Existing { get; } = [];
    public List<CancellationToken> Tokens { get; } = [];
    public int SaveCalls { get; private set; }
    public int ReadCalls { get; private set; }
    public Guid? QueriedUserId { get; private set; }
    public Guid? QueriedId { get; private set; }
    public (int Page, int PageSize)? QueriedPage { get; private set; }
    public PaymentPage HistoryResult { get; set; } = new([], 0);
    public Exception? SaveFailure { get; set; }

    public Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Tokens.Add(cancellationToken);
        Added.Add(payment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Tokens.Add(cancellationToken);
        SaveCalls++;
        return SaveFailure is null ? Task.CompletedTask : Task.FromException(SaveFailure);
    }

    public Task<Payment?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Tokens.Add(cancellationToken);
        ReadCalls++;
        QueriedId = id;
        QueriedUserId = userId;
        return Task.FromResult(Existing.SingleOrDefault(payment => payment.Id == id && payment.UserId == userId));
    }

    public Task<PaymentPage> GetByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Tokens.Add(cancellationToken);
        ReadCalls++;
        QueriedUserId = userId;
        QueriedPage = (page, pageSize);
        return Task.FromResult(HistoryResult);
    }
}

internal sealed class StubPaymentCardVerifier : IPaymentCardVerifier
{
    public HashSet<(Guid CardId, Guid UserId)> Allowed { get; } = [];
    public int Calls { get; private set; }
    public CancellationToken LastToken { get; private set; }

    public Task<bool> BelongsToUserAsync(Guid cardId, Guid userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls++;
        LastToken = cancellationToken;
        return Task.FromResult(Allowed.Contains((cardId, userId)));
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal static class PaymentSamples
{
    public static readonly DateTimeOffset Now = new(2030, 1, 15, 12, 0, 0, TimeSpan.Zero);

    public static Payment For(Guid userId) => Payment.Register(
        userId, Guid.NewGuid(), 12500.50m, "COP", "Pago registrado", Now);
}
