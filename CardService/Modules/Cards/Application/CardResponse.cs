using TuyaCardFlow.Modules.Cards.Domain;

namespace TuyaCardFlow.Modules.Cards.Application;

public sealed record CardResponse(
    Guid Id,
    Guid UserId,
    string HolderName,
    string Brand,
    string Last4,
    int ExpirationMonth,
    int ExpirationYear)
{
    internal static CardResponse From(Card card) => new(
        card.Id, card.UserId, card.HolderName, card.Brand, card.Last4,
        card.ExpirationMonth, card.ExpirationYear);
}
