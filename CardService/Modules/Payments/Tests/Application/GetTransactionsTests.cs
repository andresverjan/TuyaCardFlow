using TuyaCardFlow.Modules.Payments.Application;
using TuyaCardFlow.Modules.Payments.Application.UseCases;
using TuyaCardFlow.Modules.Payments.Domain;
using TuyaCardFlow.Modules.Payments.Tests.TestDoubles;

namespace TuyaCardFlow.Modules.Payments.Tests.Application;

public sealed class GetTransactionsTests
{
    [Fact]
    public async Task Detail_returns_the_requested_users_transaction_without_writing()
    {
        var repository = new StubPaymentRepository();
        var payment = PaymentSamples.For(Guid.NewGuid());
        repository.Existing.Add(payment);
        using var cancellation = new CancellationTokenSource();

        var result = await new GetTransactionById(repository)
            .ExecuteAsync(payment.UserId, payment.Id, cancellation.Token);

        Assert.Equal(payment.Id, result.Id);
        Assert.Equal(payment.UserId, result.UserId);
        Assert.Equal(payment.CardId, result.CardId);
        Assert.Equal(payment.Amount, result.Amount);
        Assert.Equal(payment.Currency, result.Currency);
        Assert.Equal(payment.OccurredAtUtc, result.OccurredAtUtc);
        Assert.Equal(payment.UserId, repository.QueriedUserId);
        Assert.Equal(payment.Id, repository.QueriedId);
        Assert.All(repository.Tokens, token => Assert.Equal(cancellation.Token, token));
        Assert.Equal(0, repository.SaveCalls);
        Assert.Empty(repository.Added);
    }

    [Fact]
    public async Task Detail_does_not_return_a_transaction_owned_by_another_user()
    {
        var repository = new StubPaymentRepository();
        var payment = PaymentSamples.For(Guid.NewGuid());
        repository.Existing.Add(payment);

        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            new GetTransactionById(repository).ExecuteAsync(Guid.NewGuid(), payment.Id));

        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Missing_transaction_returns_not_found()
    {
        await Assert.ThrowsAsync<TransactionNotFoundException>(() =>
            new GetTransactionById(new StubPaymentRepository()).ExecuteAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Detail_requires_user_and_transaction_before_reading(bool emptyUser)
    {
        var repository = new StubPaymentRepository();
        await Assert.ThrowsAsync<PaymentValidationException>(() => new GetTransactionById(repository).ExecuteAsync(
            emptyUser ? Guid.Empty : Guid.NewGuid(), emptyUser ? Guid.NewGuid() : Guid.Empty));
        Assert.Equal(0, repository.ReadCalls);
    }

    [Fact]
    public async Task History_returns_the_requested_page_and_maps_the_repository_result()
    {
        var userId = Guid.NewGuid();
        var first = PaymentSamples.For(userId);
        var second = PaymentSamples.For(userId);
        var repository = new StubPaymentRepository { HistoryResult = new PaymentPage([first, second], 41) };
        using var cancellation = new CancellationTokenSource();

        var result = await new GetUserTransactions(repository).ExecuteAsync(userId, 2, 2, cancellation.Token);

        Assert.Equal(userId, repository.QueriedUserId);
        Assert.Equal((2, 2), repository.QueriedPage);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(41, result.TotalCount);
        Assert.Equal(new[] { first.Id, second.Id }, result.Items.Select(item => item.Id));
        Assert.All(result.Items, item => Assert.Equal(userId, item.UserId));
        Assert.All(repository.Tokens, token => Assert.Equal(cancellation.Token, token));
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task User_without_transactions_receives_an_empty_history_with_default_pagination()
    {
        var repository = new StubPaymentRepository();
        var result = await new GetUserTransactions(repository).ExecuteAsync(Guid.NewGuid());

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
        var repository = new StubPaymentRepository();
        await Assert.ThrowsAsync<PaymentValidationException>(() =>
            new GetUserTransactions(repository).ExecuteAsync(Guid.NewGuid(), page, pageSize));
        Assert.Equal(0, repository.ReadCalls);
    }

    [Fact]
    public async Task History_requires_a_user_before_reading()
    {
        var repository = new StubPaymentRepository();
        await Assert.ThrowsAsync<PaymentValidationException>(() =>
            new GetUserTransactions(repository).ExecuteAsync(Guid.Empty));
        Assert.Equal(0, repository.ReadCalls);
    }
}
