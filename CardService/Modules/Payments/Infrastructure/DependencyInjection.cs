using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TuyaCardFlow.Modules.Payments.Application;
using TuyaCardFlow.Modules.Payments.Infrastructure.Cards;
using TuyaCardFlow.Modules.Payments.Infrastructure.Persistence;
using TuyaCardFlow.Modules.Payments.Infrastructure.Persistence.Repositories;

namespace TuyaCardFlow.Modules.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PaymentDbContext>(options => options.UseNpgsql(connectionString,
            postgres => postgres.MigrationsHistoryTable("__PaymentsMigrationsHistory")));
        services.AddScoped<IPaymentRepository, EfPaymentRepository>();
        services.AddScoped<IPaymentCardVerifier, CardModulePaymentCardVerifier>();
        return services;
    }
}
