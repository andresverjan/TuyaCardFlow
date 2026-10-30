namespace TuyaCardFlow.Modules.Cards.Domain;

public sealed class Card
{
    private Card() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string HolderName { get; private set; } = string.Empty;
    public string Brand { get; private set; } = string.Empty;
    public string Last4 { get; private set; } = string.Empty;
    public int ExpirationMonth { get; private set; }
    public int ExpirationYear { get; private set; }

    public static Card Create(
        Guid userId,
        string holderName,
        string brand,
        string last4,
        int expirationMonth,
        int expirationYear)
    {
        if (userId == Guid.Empty)
            throw new CardValidationException("El usuario propietario es obligatorio.");

        var card = new Card { Id = Guid.NewGuid(), UserId = userId };
        card.Update(holderName, brand, last4, expirationMonth, expirationYear);
        return card;
    }

    public void Update(
        string holderName,
        string brand,
        string last4,
        int expirationMonth,
        int expirationYear)
    {
        // Validate everything before changing state, including when updating an existing card.
        var normalizedHolderName = RequiredText(holderName, 150, "El titular");
        var normalizedBrand = RequiredText(brand, 50, "La marca");

        if (last4 is null || last4.Length != 4 || last4.Any(digit => digit is < '0' or > '9'))
            throw new CardValidationException("Last4 debe contener exactamente cuatro dígitos.");

        if (expirationMonth is < 1 or > 12)
            throw new CardValidationException("El mes de vencimiento debe estar entre 1 y 12.");

        if (expirationYear is < 2000 or > 9999)
            throw new CardValidationException("El año de vencimiento debe estar entre 2000 y 9999.");

        HolderName = normalizedHolderName;
        Brand = normalizedBrand;
        Last4 = last4;
        ExpirationMonth = expirationMonth;
        ExpirationYear = expirationYear;
    }

    private static string RequiredText(string value, int maximumLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > maximumLength)
            throw new CardValidationException($"{field} es obligatorio y admite hasta {maximumLength} caracteres.");

        return value.Trim();
    }
}
