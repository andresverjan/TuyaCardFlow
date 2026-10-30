namespace TuyaCardFlow.Modules.Cards.Domain;

public sealed class CardValidationException(string message) : Exception(message);
