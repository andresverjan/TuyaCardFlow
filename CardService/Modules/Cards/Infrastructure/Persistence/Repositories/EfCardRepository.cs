using TuyaCardFlow.Modules.Cards.Application;
using TuyaCardFlow.Modules.Cards.Domain;
using TuyaCardFlow.Modules.Cards.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TuyaCardFlow.Modules.Cards.Infrastructure.Persistence.Repositories;

public sealed class EfCardRepository(CardDbContext dbContext) : ICardRepository
{
    public Task<Card?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Cards.SingleOrDefaultAsync(card => card.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Card>> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await dbContext.Cards
            .AsNoTracking()
            .Where(card => card.UserId == userId)
            .OrderBy(card => card.Id)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Card card, CancellationToken cancellationToken = default) =>
        await dbContext.Cards.AddAsync(card, cancellationToken);

    public void Remove(Card card) => dbContext.Cards.Remove(card);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
