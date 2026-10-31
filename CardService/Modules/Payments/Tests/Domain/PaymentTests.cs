using System.Globalization;
using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Tests.Domain;

public sealed class PaymentTests
{
    private static readonly DateTimeOffset RecordedAt = new(2030, 1, 15, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Registration_requires_both_user_and_card(bool emptyUser)
    {
        Assert.Throws<PaymentValidationException>(() => Payment.Register(
            emptyUser ? Guid.Empty : Guid.NewGuid(), emptyUser ? Guid.NewGuid() : Guid.Empty,
            100m, "COP", null, RecordedAt));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("0.001")]
    [InlineData("120.999")]
    [InlineData("10000000000000000")]
    public void Invalid_amount_is_rejected_without_rounding(string amount)
    {
        var value = decimal.Parse(amount, CultureInfo.InvariantCulture);
        Assert.Throws<PaymentValidationException>(() => Payment.Register(
            Guid.NewGuid(), Guid.NewGuid(), value, "COP", null, RecordedAt));
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("9999999999999999.99")]
    public void Amount_boundaries_are_preserved_exactly(string amount)
    {
        var value = decimal.Parse(amount, CultureInfo.InvariantCulture);
        var payment = Payment.Register(Guid.NewGuid(), Guid.NewGuid(), value, "COP", null, RecordedAt);
        Assert.Equal(value, payment.Amount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("CO")]
    [InlineData("COPP")]
    [InlineData("C0P")]
    [InlineData("CÓP")]
    public void Currency_requires_three_ascii_letters(string? currency)
    {
        Assert.Throws<PaymentValidationException>(() => Payment.Register(
            Guid.NewGuid(), Guid.NewGuid(), 100m, currency!, null, RecordedAt));
    }

    [Fact]
    public void Registration_normalizes_text_and_time_without_changing_ownership_or_amount()
    {
        var userId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var localTime = new DateTimeOffset(2030, 1, 15, 7, 0, 0, TimeSpan.FromHours(-5));
        var payment = Payment.Register(userId, cardId, 123.45m, " cop ", "  Pago de tarjeta  ", localTime);

        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(userId, payment.UserId);
        Assert.Equal(cardId, payment.CardId);
        Assert.Equal(123.45m, payment.Amount);
        Assert.Equal("COP", payment.Currency);
        Assert.Equal("Pago de tarjeta", payment.Description);
        Assert.Equal(RecordedAt, payment.OccurredAtUtc);
        Assert.Equal(TimeSpan.Zero, payment.OccurredAtUtc.Offset);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Optional_blank_description_is_stored_as_null(string? description)
    {
        var payment = Payment.Register(Guid.NewGuid(), Guid.NewGuid(), 100m, "COP", description, RecordedAt);
        Assert.Null(payment.Description);
    }

    [Fact]
    public void Description_over_the_limit_is_rejected()
    {
        Assert.Throws<PaymentValidationException>(() => Payment.Register(
            Guid.NewGuid(), Guid.NewGuid(), 100m, "COP", new string('x', 251), RecordedAt));
    }

    [Fact]
    public void Missing_recording_time_is_rejected()
    {
        Assert.Throws<PaymentValidationException>(() => Payment.Register(
            Guid.NewGuid(), Guid.NewGuid(), 100m, "COP", null, default));
    }
}
