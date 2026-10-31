using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Application.UseCases;
using TuyaCardFlow.Modules.Users.Domain;
using TuyaCardFlow.Modules.Users.Tests.TestDoubles;

namespace TuyaCardFlow.Modules.Users.Tests.Application;

public sealed class WriteUsersTests
{
    [Fact]
    public async Task Create_stores_a_normalized_active_profile_with_the_server_time()
    {
        var repository = new StubUserRepository();
        using var cancellation = new CancellationTokenSource();

        var result = await new CreateUser(repository, Clock(), new StubUserPasswordService()).ExecuteAsync(
            new CreateUserCommand("  Ana Pérez ", " ANA@Example.COM "), cancellation.Token);

        var user = Assert.Single(repository.Added);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("Ana Pérez", result.FullName);
        Assert.Equal("ana@example.com", result.Email);
        Assert.True(result.IsActive);
        Assert.Equal(UserSamples.Now, result.CreatedAtUtc);
        Assert.Equal(UserSamples.Now, result.UpdatedAtUtc);
        Assert.Equal("ANA@EXAMPLE.COM", repository.QueriedEmail);
        Assert.Equal(1, repository.SaveCalls);
        Assert.All(repository.Tokens, token => Assert.Equal(cancellation.Token, token));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Duplicate_email_is_rejected_even_for_an_inactive_user(bool existingIsActive)
    {
        var repository = new StubUserRepository();
        var existing = UserSamples.Create();
        existing.SetActive(existingIsActive, UserSamples.Now);
        repository.Existing.Add(existing);

        await Assert.ThrowsAsync<DuplicateUserEmailException>(() =>
            new CreateUser(repository, Clock(), new StubUserPasswordService()).ExecuteAsync(new CreateUserCommand("Otra persona", " ANA@EXAMPLE.COM ")));

        Assert.Empty(repository.Added);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Theory]
    [InlineData("", "ana@example.com")]
    [InlineData("Ana Pérez", "incorrecto")]
    public async Task Invalid_creation_is_rejected_before_any_repository_call(string name, string email)
    {
        var repository = new StubUserRepository();
        await Assert.ThrowsAsync<UserValidationException>(() => new CreateUser(repository, Clock(), new StubUserPasswordService())
            .ExecuteAsync(new CreateUserCommand(name, email)));
        Assert.Empty(repository.Tokens);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Concurrent_email_conflict_at_save_is_propagated()
    {
        var failure = new DuplicateUserEmailException();
        var repository = new StubUserRepository { SaveFailure = failure };

        var exception = await Assert.ThrowsAsync<DuplicateUserEmailException>(() =>
            new CreateUser(repository, Clock(), new StubUserPasswordService()).ExecuteAsync(new CreateUserCommand("Ana Pérez", "ana@example.com")));

        Assert.Same(failure, exception);
    }

    [Fact]
    public async Task Save_failure_does_not_return_a_successful_user()
    {
        var failure = new InvalidOperationException("Error de persistencia.");
        var repository = new StubUserRepository { SaveFailure = failure };
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new CreateUser(repository, Clock(), new StubUserPasswordService()).ExecuteAsync(new CreateUserCommand("Ana Pérez", "ana@example.com")));
        Assert.Same(failure, exception);
    }

    [Fact]
    public async Task Update_allows_retaining_own_email_and_preserves_creation_time()
    {
        var repository = new StubUserRepository();
        var user = UserSamples.Create();
        repository.Existing.Add(user);
        using var cancellation = new CancellationTokenSource();

        var result = await new UpdateUser(repository, Clock(UserSamples.Now.AddHours(1))).ExecuteAsync(user.Id,
            new UpdateUserCommand("Ana María", "ANA@EXAMPLE.COM"), cancellation.Token);

        Assert.Equal("Ana María", result.FullName);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(UserSamples.Now, result.CreatedAtUtc);
        Assert.Equal(UserSamples.Now.AddHours(1), result.UpdatedAtUtc);
        Assert.Equal(user.Id, repository.ExcludedId);
        Assert.Equal(1, repository.SaveCalls);
        Assert.All(repository.Tokens, token => Assert.Equal(cancellation.Token, token));
    }

    [Fact]
    public async Task Conflicting_update_does_not_mutate_existing_user()
    {
        var repository = new StubUserRepository();
        var user = UserSamples.Create();
        repository.Existing.AddRange([user, UserSamples.Create("otro@example.com")]);

        await Assert.ThrowsAsync<DuplicateUserEmailException>(() => new UpdateUser(repository, Clock()).ExecuteAsync(
            user.Id, new UpdateUserCommand("Otro nombre", "OTRO@EXAMPLE.COM")));

        Assert.Equal("Ana Pérez", user.FullName);
        Assert.Equal("ana@example.com", user.Email);
        Assert.Equal(UserSamples.Now, user.UpdatedAtUtc);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Invalid_update_is_rejected_before_reading()
    {
        var repository = new StubUserRepository();
        await Assert.ThrowsAsync<UserValidationException>(() => new UpdateUser(repository, Clock()).ExecuteAsync(
            Guid.NewGuid(), new UpdateUserCommand("", "ana@example.com")));
        Assert.Equal(0, repository.ReadCalls);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Updating_unknown_user_returns_not_found_without_saving()
    {
        var repository = new StubUserRepository();
        await Assert.ThrowsAsync<UserNotFoundException>(() => new UpdateUser(repository, Clock()).ExecuteAsync(
            Guid.NewGuid(), new UpdateUserCommand("Ana Pérez", "ana@example.com")));
        Assert.Equal(0, repository.EmailChecks);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Status_change_preserves_user_identity_and_profile(bool isActive)
    {
        var repository = new StubUserRepository();
        var user = UserSamples.Create();
        user.SetActive(!isActive, UserSamples.Now);
        repository.Existing.Add(user);

        var result = await new SetUserStatus(repository, Clock(UserSamples.Now.AddHours(1)))
            .ExecuteAsync(user.Id, isActive);

        Assert.Equal(isActive, result.IsActive);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("ana@example.com", result.Email);
        Assert.Equal(UserSamples.Now, result.CreatedAtUtc);
        Assert.Single(repository.Existing);
        Assert.Equal(1, repository.SaveCalls);
    }

    [Fact]
    public async Task Unknown_status_target_returns_not_found_without_saving()
    {
        var repository = new StubUserRepository();
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            new SetUserStatus(repository, Clock()).ExecuteAsync(Guid.NewGuid(), false));
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Empty_ids_are_rejected_before_reading_or_saving()
    {
        var repository = new StubUserRepository();
        await Assert.ThrowsAsync<UserValidationException>(() => new UpdateUser(repository, Clock()).ExecuteAsync(
            Guid.Empty, new UpdateUserCommand("Ana Pérez", "ana@example.com")));
        await Assert.ThrowsAsync<UserValidationException>(() =>
            new SetUserStatus(repository, Clock()).ExecuteAsync(Guid.Empty, false));
        Assert.Equal(0, repository.ReadCalls);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Cancelled_creation_does_not_add_or_save_a_user()
    {
        var repository = new StubUserRepository();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new CreateUser(repository, Clock(), new StubUserPasswordService())
            .ExecuteAsync(new CreateUserCommand("Ana Pérez", "ana@example.com"), cancellation.Token));
        Assert.Empty(repository.Added);
        Assert.Equal(0, repository.SaveCalls);
    }

    private static FixedTimeProvider Clock(DateTimeOffset? now = null) => new(now ?? UserSamples.Now);
}
