namespace TuyaCardFlow.Modules.Cards.Application;

public sealed record CreateCardCommand(
    Guid UserId,
    string HolderName,
    string Brand,
    string Last4,
    int ExpirationMonth,
    int ExpirationYear);
