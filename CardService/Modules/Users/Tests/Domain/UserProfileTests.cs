using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Tests.Domain;

public sealed class UserProfileTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Ana\nPérez")]
    public void Invalid_name_is_rejected(string? name)
    {
        Assert.Throws<UserValidationException>(() => UserProfile.Create(name!, "ana@example.com"));
    }

    [Fact]
    public void Name_length_is_limited_to_150_characters()
    {
        Assert.Throws<UserValidationException>(() => UserProfile.Create(new string('A', 151), "ana@example.com"));
        Assert.Equal(150, UserProfile.Create(new string('A', 150), "ana@example.com").FullName.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("sin-arroba")]
    [InlineData("ana@@example.com")]
    [InlineData("ana @example.com")]
    [InlineData("<ana@example.com>")]
    [InlineData("Ana <ana@example.com>")]
    [InlineData("ana@example.com,otro@example.com")]
    public void Invalid_or_non_plain_email_is_rejected(string? email)
    {
        Assert.Throws<UserValidationException>(() => UserProfile.Create("Ana Pérez", email!));
    }

    [Fact]
    public void Email_length_is_limited_to_254_characters()
    {
        Assert.Throws<UserValidationException>(() =>
            UserProfile.Create("Ana Pérez", new string('a', 255) + "@example.com"));
    }

    [Fact]
    public void Profile_normalizes_email_and_outer_spaces_without_losing_unicode_names()
    {
        var profile = UserProfile.Create("  Ana María Pérez  ", "  ANA+TUYA@Example.COM  ");

        Assert.Equal("Ana María Pérez", profile.FullName);
        Assert.Equal("ana+tuya@example.com", profile.Email);
        Assert.Equal("ANA+TUYA@EXAMPLE.COM", profile.NormalizedEmail);
    }
}
