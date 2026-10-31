namespace TuyaCardFlow.Modules.Payments.Application;

public sealed class PaymentCardNotFoundException(Guid cardId)
    : Exception($"No se encontró la tarjeta '{cardId}' para el usuario indicado.");
