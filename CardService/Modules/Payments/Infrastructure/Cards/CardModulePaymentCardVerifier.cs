using TuyaCardFlow.Modules.Cards.Application;
using TuyaCardFlow.Modules.Cards.Application.UseCases;
using TuyaCardFlow.Modules.Payments.Application;

namespace TuyaCardFlow.Modules.Payments.Infrastructure.Cards;

public sealed class CardModulePaymentCardVerifier(GetCardById getCardById) : IPaymentCardVerifier
{
    public async Task<bool> BelongsToUserAsync(
        Guid cardId, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var card = await getCardById.ExecuteAsync(cardId, cancellationToken);
            return card.UserId == userId;
        }
        catch (CardNotFoundException)
        {
            return false;
        }
    }
}
