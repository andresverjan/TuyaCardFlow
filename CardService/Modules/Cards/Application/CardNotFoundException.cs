namespace TuyaCardFlow.Modules.Cards.Application;

public sealed class CardNotFoundException(Guid cardId)
    : Exception($"No se encontró la tarjeta '{cardId}'.");
