using TuyaCardFlow.Modules.Cards.Domain;

namespace TuyaCardFlow.Modules.Cards.Application.UseCases;

public sealed class GetUserCards(ICardRepository repository)
{
    public async Task<IReadOnlyList<CardResponse>> ExecuteAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new CardValidationException("El usuario es obligatorio.");

        var cards = await repository.GetByUserIdAsync(userId, cancellationToken);
        return cards.Select(CardResponse.From).ToArray();
    }
}
