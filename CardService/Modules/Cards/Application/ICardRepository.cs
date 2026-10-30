using TuyaCardFlow.Modules.Cards.Domain;

namespace TuyaCardFlow.Modules.Cards.Application;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Card>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Card card, CancellationToken cancellationToken = default);
    void Remove(Card card);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
