using Microsoft.EntityFrameworkCore;
using TuyaCardFlow.Modules.Payments.Application;
using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Infrastructure.Persistence.Repositories;

public sealed class EfPaymentRepository(PaymentDbContext dbContext) : IPaymentRepository
{
    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default) =>
        await dbContext.Payments.AddAsync(payment, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public Task<Payment?> GetByIdForUserAsync(
        Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        dbContext.Payments.AsNoTracking().SingleOrDefaultAsync(
            payment => payment.Id == id && payment.UserId == userId, cancellationToken);

    public async Task<PaymentPage> GetByUserAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Payments.AsNoTracking().Where(payment => payment.UserId == userId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(payment => payment.OccurredAtUtc)
            .ThenByDescending(payment => payment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PaymentPage(items, totalCount);
    }
}
