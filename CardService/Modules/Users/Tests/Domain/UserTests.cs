using TuyaCardFlow.Modules.Users.Domain;
using TuyaCardFlow.Modules.Users.Tests.TestDoubles;

namespace TuyaCardFlow.Modules.Users.Tests.Domain;

public sealed class UserTests
{
    [Fact]
    public void User_starts_active_with_server_timestamps_in_utc()
    {
        var localTime = new DateTimeOffset(2030, 1, 15, 7, 0, 0, TimeSpan.FromHours(-5));
        var user = User.Create(UserProfile.Create("Ana Pérez", "ana@example.com"), localTime);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.True(user.IsActive);
        Assert.Equal(UserSamples.Now, user.CreatedAtUtc);
        Assert.Equal(user.CreatedAtUtc, user.UpdatedAtUtc);
        Assert.Equal(TimeSpan.Zero, user.CreatedAtUtc.Offset);
    }

    [Fact]
    public void Updating_an_inactive_profile_preserves_identity_creation_time_and_status()
    {
        var user = UserSamples.Create();
        var id = user.Id;
        user.SetActive(false, UserSamples.Now.AddMinutes(1));

        user.UpdateProfile(UserProfile.Create("Ana María", "nueva@example.com"), UserSamples.Now.AddHours(1));

        Assert.Equal(id, user.Id);
        Assert.False(user.IsActive);
        Assert.Equal(UserSamples.Now, user.CreatedAtUtc);
        Assert.Equal(UserSamples.Now.AddHours(1), user.UpdatedAtUtc);
        Assert.Equal("Ana María", user.FullName);
        Assert.Equal("nueva@example.com", user.Email);
        Assert.Equal("NUEVA@EXAMPLE.COM", user.NormalizedEmail);
    }

    [Fact]
    public void Deactivation_and_reactivation_preserve_the_profile_and_id()
    {
        var user = UserSamples.Create();
        var id = user.Id;

        user.SetActive(false, UserSamples.Now.AddMinutes(1));
        Assert.False(user.IsActive);
        user.SetActive(true, UserSamples.Now.AddMinutes(2));

        Assert.True(user.IsActive);
        Assert.Equal(id, user.Id);
        Assert.Equal("ana@example.com", user.Email);
        Assert.Equal(UserSamples.Now, user.CreatedAtUtc);
        Assert.Equal(UserSamples.Now.AddMinutes(2), user.UpdatedAtUtc);
    }

    [Fact]
    public void Repeating_deactivation_is_idempotent()
    {
        var user = UserSamples.Create();
        user.SetActive(false, UserSamples.Now.AddMinutes(1));
        user.SetActive(false, UserSamples.Now.AddMinutes(10));
        Assert.Equal(UserSamples.Now.AddMinutes(1), user.UpdatedAtUtc);
    }

    [Fact]
    public void Invalid_update_timestamp_leaves_existing_profile_unchanged()
    {
        var user = UserSamples.Create();
        Assert.Throws<UserValidationException>(() => user.UpdateProfile(
            UserProfile.Create("Otro nombre", "otra@example.com"), default));
        Assert.Equal("Ana Pérez", user.FullName);
        Assert.Equal("ana@example.com", user.Email);
        Assert.Equal(UserSamples.Now, user.UpdatedAtUtc);
    }

    [Fact]
    public void Creation_requires_a_timestamp()
    {
        Assert.Throws<UserValidationException>(() =>
            User.Create(UserProfile.Create("Ana Pérez", "ana@example.com"), default));
    }
}
