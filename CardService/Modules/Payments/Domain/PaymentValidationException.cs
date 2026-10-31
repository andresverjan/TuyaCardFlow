namespace TuyaCardFlow.Modules.Payments.Domain;

public sealed class PaymentValidationException(string message) : Exception(message);
