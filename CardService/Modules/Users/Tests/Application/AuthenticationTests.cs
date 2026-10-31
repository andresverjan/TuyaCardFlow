using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Application.UseCases;
using TuyaCardFlow.Modules.Users.Domain;
using TuyaCardFlow.Modules.Users.Tests.TestDoubles;

namespace TuyaCardFlow.Modules.Users.Tests.Application;

public sealed class AuthenticationTests
{
    [Fact]
    public async Task Registration_stores_a_hash_and_preserves_the_password_free_response()
    {
        var repository = new StubUserRepository();
        var service = new StubUserPasswordService();
        var result = await new CreateUser(repository, Clock(), service)
            .ExecuteAsync(new CreateUserCommand("Ana Pérez", "ana@example.com", "una frase muy segura"));
        var user = Assert.Single(repository.Added);
        Assert.Equal("test-only-opaque-hash", user.PasswordHash);
        Assert.NotEqual("una frase muy segura", user.PasswordHash);
        Assert.DoesNotContain("Password", string.Join(",", typeof(UserResponse).GetProperties().Select(property => property.Name)));
        Assert.Equal(user.Id, result.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("corta")]
    [InlineData("            ")]
    public async Task Invalid_password_is_rejected_before_reading_or_saving(string password)
    {
        var repository = new StubUserRepository();
        await Assert.ThrowsAsync<UserValidationException>(() => new CreateUser(repository, Clock(), new StubUserPasswordService())
            .ExecuteAsync(new CreateUserCommand("Ana Pérez", "ana@example.com", password)));
        Assert.Empty(repository.Tokens);
    }

    [Fact]
    public async Task Login_returns_the_active_user_and_its_current_session_version()
    {
        var repository = new StubUserRepository();
        var user = Account(); repository.Existing.Add(user);
        var result = await new LoginUser(repository, new StubUserPasswordService(), Clock())
            .ExecuteAsync(new LoginUserCommand("ANA@EXAMPLE.COM", "una frase muy segura"));
        Assert.Equal(user.Id, result.User.Id);
        Assert.Equal(user.AuthenticationVersion, result.AuthenticationVersion);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("inactive")]
    [InlineData("wrong")]
    [InlineData("no-password")]
    public async Task Invalid_accounts_receive_the_same_generic_credential_error(string scenario)
    {
        var repository = new StubUserRepository();
        var user = scenario == "no-password" ? UserSamples.Create() : Account();
        if (scenario == "inactive") user.SetActive(false, UserSamples.Now);
        if (scenario != "missing") repository.Existing.Add(user);
        var passwords = new StubUserPasswordService { Valid = scenario != "wrong" };
        var exception = await Assert.ThrowsAsync<InvalidUserCredentialsException>(() => new LoginUser(repository, passwords, Clock())
            .ExecuteAsync(new LoginUserCommand("ana@example.com", "una frase muy segura")));
        Assert.Equal("El correo o la contraseña no son correctos.", exception.Message);
        Assert.Equal(1, passwords.VerifyCalls);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Rehash_uses_the_password_adapter_and_updates_the_session_version()
    {
        var repository = new StubUserRepository(); var user = Account(); repository.Existing.Add(user);
        var old = user.AuthenticationVersion;
        var result = await new LoginUser(repository, new StubUserPasswordService { NeedsRehash = true }, Clock())
            .ExecuteAsync(new LoginUserCommand("ana@example.com", "una frase muy segura"));
        Assert.Equal(1, repository.SaveCalls);
        Assert.NotEqual(old, result.AuthenticationVersion);
    }

    [Fact]
    public async Task Deactivation_then_reactivation_does_not_restore_old_sessions()
    {
        var repository = new StubUserRepository(); var user = Account(); repository.Existing.Add(user);
        var old = user.AuthenticationVersion;
        var validate = new ValidateUserSession(repository);
        Assert.NotNull(await validate.ExecuteAsync(user.Id, old));
        user.SetActive(false, UserSamples.Now);
        Assert.Null(await validate.ExecuteAsync(user.Id, old));
        user.SetActive(true, UserSamples.Now);
        Assert.Null(await validate.ExecuteAsync(user.Id, old));
    }

    [Fact]
    public async Task Email_change_invalidates_the_old_session()
    {
        var repository = new StubUserRepository(); var user = Account(); repository.Existing.Add(user);
        var old = user.AuthenticationVersion;
        user.UpdateProfile(UserProfile.Create("Ana Pérez", "otra@example.com"), UserSamples.Now);
        Assert.Null(await new ValidateUserSession(repository).ExecuteAsync(user.Id, old));
    }

    [Fact]
    public async Task Password_change_invalidates_the_old_session()
    {
        var repository = new StubUserRepository(); var user = Account(); repository.Existing.Add(user);
        var old = user.AuthenticationVersion;
        user.SetPasswordHash("different-opaque-hash", UserSamples.Now);
        Assert.Null(await new ValidateUserSession(repository).ExecuteAsync(user.Id, old));
    }

    [Fact]
    public async Task Missing_session_user_returns_null()
    {
        Assert.Null(await new ValidateUserSession(new StubUserRepository()).ExecuteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    private static FixedTimeProvider Clock() => new(UserSamples.Now);
    private static User Account()
    {
        var user = UserSamples.Create(); user.SetPasswordHash("opaque-test-hash", UserSamples.Now); return user;
    }
}
