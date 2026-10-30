using TuyaCardFlow.Modules.Cards.Application;
using TuyaCardFlow.Modules.Cards.Infrastructure.Persistence.Repositories;
using TuyaCardFlow.Modules.Cards.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TuyaCardFlow.Modules.Cards.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCardsInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CardDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICardRepository, EfCardRepository>();
        return services;
    }
}
