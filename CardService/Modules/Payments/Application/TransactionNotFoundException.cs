namespace TuyaCardFlow.Modules.Payments.Application;

public sealed class TransactionNotFoundException(Guid transactionId)
    : Exception($"No se encontró la transacción '{transactionId}' para el usuario indicado.");
