using TuyaCardFlow.Modules.Cards.Domain;

namespace TuyaCardFlow.Modules.Cards.Tests.Domain;

public sealed class CardTests
{
    [Fact]
    public void Create_requires_a_nonempty_owner()
    {
        Assert.Throws<CardValidationException>(() =>
            Card.Create(Guid.Empty, "Ana Pérez", "Tuya", "1234", 12, 2030));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("1234567890123456")]
    [InlineData("12a4")]
    [InlineData("１２３４")]
    public void Create_accepts_only_four_ascii_digits(string last4)
    {
        Assert.Throws<CardValidationException>(() =>
            Card.Create(Guid.NewGuid(), "Ana Pérez", "Tuya", last4, 12, 2030));
    }

    [Fact]
    public void Invalid_update_leaves_existing_card_unchanged()
    {
        var card = Card.Create(Guid.NewGuid(), "Ana Pérez", "Tuya", "1234", 12, 2030);
        var originalId = card.Id;
        var originalUserId = card.UserId;

        Assert.Throws<CardValidationException>(() =>
            card.Update("Otro titular", "Otra marca", "5678", 13, 2035));

        Assert.Equal(originalId, card.Id);
        Assert.Equal(originalUserId, card.UserId);
        Assert.Equal("Ana Pérez", card.HolderName);
        Assert.Equal("Tuya", card.Brand);
        Assert.Equal("1234", card.Last4);
        Assert.Equal(12, card.ExpirationMonth);
        Assert.Equal(2030, card.ExpirationYear);
    }
}
