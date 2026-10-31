using TuyaCardFlow.Modules.Payments.Application;
using TuyaCardFlow.Modules.Payments.Application.UseCases;
using TuyaCardFlow.Modules.Payments.Domain;
using TuyaCardFlow.Modules.Payments.Tests.TestDoubles;

namespace TuyaCardFlow.Modules.Payments.Tests.Application;

public sealed class RegisterPaymentTests
{
    [Fact]
    public async Task Valid_payment_is_saved_with_the_server_time_and_returned_as_a_transaction()
    {
        var repository = new StubPaymentRepository();
        var verifier = new StubPaymentCardVerifier();
        var command = ValidCommand();
        verifier.Allowed.Add((command.CardId, command.UserId));
        var useCase = new RegisterPayment(repository, verifier, new FixedTimeProvider(PaymentSamples.Now));
        using var cancellation = new CancellationTokenSource();

        var result = await useCase.ExecuteAsync(command, cancellation.Token);

        var stored = Assert.Single(repository.Added);
        Assert.Equal(1, repository.SaveCalls);
        Assert.Equal(stored.Id, result.Id);
        Assert.Equal(command.UserId, result.UserId);
        Assert.Equal(command.CardId, result.CardId);
        Assert.Equal(command.Amount, result.Amount);
        Assert.Equal("COP", result.Currency);
        Assert.Equal("Pago de tarjeta", result.Description);
        Assert.Equal(PaymentSamples.Now, result.OccurredAtUtc);
        Assert.Equal(cancellation.Token, verifier.LastToken);
        Assert.All(repository.Tokens, token => Assert.Equal(cancellation.Token, token));
    }

    [Fact]
    public async Task Unknown_card_is_rejected_before_any_payment_is_saved()
    {
        var repository = new StubPaymentRepository();
        var useCase = new RegisterPayment(repository, new StubPaymentCardVerifier(), new FixedTimeProvider(PaymentSamples.Now));

        await Assert.ThrowsAsync<PaymentCardNotFoundException>(() => useCase.ExecuteAsync(ValidCommand()));

        Assert.Empty(repository.Added);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Card_belonging_to_another_user_cannot_be_used()
    {
        var repository = new StubPaymentRepository();
        var verifier = new StubPaymentCardVerifier();
        var command = ValidCommand();
        verifier.Allowed.Add((command.CardId, Guid.NewGuid()));
        var useCase = new RegisterPayment(repository, verifier, new FixedTimeProvider(PaymentSamples.Now));

        await Assert.ThrowsAsync<PaymentCardNotFoundException>(() => useCase.ExecuteAsync(command));

        Assert.Empty(repository.Added);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Theory]
    [InlineData("amount")]
    [InlineData("user")]
    [InlineData("card")]
    [InlineData("currency")]
    public async Task Invalid_payment_is_rejected_before_calling_external_ports(string invalidField)
    {
        var repository = new StubPaymentRepository();
        var verifier = new StubPaymentCardVerifier();
        var command = ValidCommand();
        command = invalidField switch
        {
            "amount" => command with { Amount = 1.001m },
            "user" => command with { UserId = Guid.Empty },
            "card" => command with { CardId = Guid.Empty },
            _ => command with { Currency = "CO" }
        };
        var useCase = new RegisterPayment(repository, verifier, new FixedTimeProvider(PaymentSamples.Now));

        await Assert.ThrowsAsync<PaymentValidationException>(() => useCase.ExecuteAsync(command));

        Assert.Equal(0, verifier.Calls);
        Assert.Empty(repository.Added);
        Assert.Equal(0, repository.SaveCalls);
    }

    [Fact]
    public async Task Save_failure_does_not_return_a_successful_transaction()
    {
        var failure = new InvalidOperationException("No se pudo guardar.");
        var repository = new StubPaymentRepository { SaveFailure = failure };
        var verifier = new StubPaymentCardVerifier();
        var command = ValidCommand();
        verifier.Allowed.Add((command.CardId, command.UserId));
        var useCase = new RegisterPayment(repository, verifier, new FixedTimeProvider(PaymentSamples.Now));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => useCase.ExecuteAsync(command));

        Assert.Same(failure, exception);
    }

    [Fact]
    public async Task Cancelled_registration_does_not_add_or_save_a_payment()
    {
        var repository = new StubPaymentRepository();
        var useCase = new RegisterPayment(repository, new StubPaymentCardVerifier(), new FixedTimeProvider(PaymentSamples.Now));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => useCase.ExecuteAsync(ValidCommand(), cancellation.Token));

        Assert.Empty(repository.Added);
        Assert.Equal(0, repository.SaveCalls);
    }

    private static RegisterPaymentCommand ValidCommand() => new(
        Guid.NewGuid(), Guid.NewGuid(), 12500.50m, "cop", "  Pago de tarjeta  ");
}
