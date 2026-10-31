using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Application.UseCases;
using TuyaCardFlow.Modules.Users.Domain;
using TuyaCardFlow.Modules.Users.Tests.TestDoubles;

namespace TuyaCardFlow.Modules.Users.Tests.Application;

public sealed class ReadUsersTests
{
    [Fact]
    public async Task Detail_returns_inactive_user_without_writing()
    {
        var repository = new StubUserRepository();
        var user = UserSamples.Create();
        user.SetActive(false, UserSamples.Now);
        repository.Existing.Add(user);

        var result = await new GetUserById(repository).ExecuteAsync(user.Id);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.FullName, result.FullName);
        Assert.Equal(user.Email, result.Email);
        Assert.False(result.IsActive);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Unknown_user_returns_not_found()
    {
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            new GetUserById(new StubUserRepository()).ExecuteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Empty_user_id_is_rejected_before_reading()
    {
        var repository = new StubUserRepository();
        await Assert.ThrowsAsync<UserValidationException>(() => new GetUserById(repository).ExecuteAsync(Guid.Empty));
        Assert.Equal(0, repository.ReadCalls);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Listing_forwards_pagination_and_status_filter_and_maps_the_result(bool? isActive)
    {
        var user = UserSamples.Create();
        var repository = new StubUserRepository { PageResult = new UserPage([user], 41) };
        using var cancellation = new CancellationTokenSource();

        var result = await new GetUsers(repository).ExecuteAsync(2, 10, isActive, cancellation.Token);

        Assert.Equal((2, 10, isActive), repository.QueriedPage);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(41, result.TotalCount);
        Assert.Equal(user.Id, Assert.Single(result.Items).Id);
        Assert.All(repository.Tokens, token => Assert.Equal(cancellation.Token, token));
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Empty_list_has_default_pagination()
    {
        var result = await new GetUsers(new StubUserRepository()).ExecuteAsync();
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public async Task Invalid_pagination_is_rejected_before_reading(int page, int pageSize)
    {
        var repository = new StubUserRepository();
        await Assert.ThrowsAsync<UserValidationException>(() => new GetUsers(repository).ExecuteAsync(page, pageSize));
        Assert.Equal(0, repository.ReadCalls);
    }
}
