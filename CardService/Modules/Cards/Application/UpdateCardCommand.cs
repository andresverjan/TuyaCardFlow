namespace TuyaCardFlow.Modules.Cards.Application;

public sealed record UpdateCardCommand(
    string HolderName,
    string Brand,
    string Last4,
    int ExpirationMonth,
    int ExpirationYear);
