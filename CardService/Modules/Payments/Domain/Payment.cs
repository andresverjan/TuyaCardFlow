namespace TuyaCardFlow.Modules.Payments.Domain;

public sealed class Payment
{
    public const decimal MaximumAmount = 9999999999999999.99m;
    public const int MaximumDescriptionLength = 250;

    private Payment() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CardId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    public static Payment Register(
        Guid userId, Guid cardId, decimal amount, string currency,
        string? description, DateTimeOffset occurredAtUtc)
    {
        if (userId == Guid.Empty)
            throw new PaymentValidationException("El usuario es obligatorio.");

        if (cardId == Guid.Empty)
            throw new PaymentValidationException("La tarjeta es obligatoria.");

        if (amount <= 0 || amount > MaximumAmount || decimal.Round(amount, 2) != amount)
            throw new PaymentValidationException(
                "El monto debe ser positivo, tener hasta dos decimales y no superar 9999999999999999.99.");

        var normalizedCurrency = currency?.Trim().ToUpperInvariant();
        if (normalizedCurrency is null || normalizedCurrency.Length != 3
            || normalizedCurrency.Any(letter => letter is < 'A' or > 'Z'))
            throw new PaymentValidationException("La moneda debe contener tres letras ASCII, por ejemplo COP.");

        var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalizedDescription?.Length > MaximumDescriptionLength)
            throw new PaymentValidationException("La descripción admite hasta 250 caracteres.");

        if (occurredAtUtc == default)
            throw new PaymentValidationException("La fecha de la transacción es obligatoria.");

        return new Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = cardId,
            Amount = amount,
            Currency = normalizedCurrency,
            Description = normalizedDescription,
            OccurredAtUtc = occurredAtUtc.ToUniversalTime()
        };
    }
}
