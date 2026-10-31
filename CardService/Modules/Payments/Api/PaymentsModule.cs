using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TuyaCardFlow.Modules.Payments.Api.Controllers;
using TuyaCardFlow.Modules.Payments.Api.Errors;
using TuyaCardFlow.Modules.Payments.Application.UseCases;
using TuyaCardFlow.Modules.Payments.Infrastructure;
using TuyaCardFlow.Modules.Payments.Infrastructure.Persistence;

namespace TuyaCardFlow.Modules.Payments.Api;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(
        this IServiceCollection services, string connectionString)
    {
        services.AddControllers().AddApplicationPart(typeof(PaymentsController).Assembly);
        services.AddExceptionHandler<PaymentsExceptionHandler>();
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<RegisterPayment>();
        services.AddScoped<GetUserTransactions>();
        services.AddScoped<GetTransactionById>();
        services.AddPaymentsInfrastructure(connectionString);
        return services;
    }

    public static async Task MigratePaymentsDatabaseAsync(
        this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
