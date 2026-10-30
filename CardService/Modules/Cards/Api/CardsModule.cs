using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TuyaCardFlow.Modules.Cards.Api.Controllers;
using TuyaCardFlow.Modules.Cards.Api.Errors;
using TuyaCardFlow.Modules.Cards.Application.UseCases;
using TuyaCardFlow.Modules.Cards.Infrastructure;
using TuyaCardFlow.Modules.Cards.Infrastructure.Persistence;

namespace TuyaCardFlow.Modules.Cards.Api;

public static class CardsModule
{
    public static IServiceCollection AddCardsModule(
        this IServiceCollection services, string connectionString)
    {
        services.AddControllers().AddApplicationPart(typeof(CardsController).Assembly);
        services.AddExceptionHandler<CardsExceptionHandler>();
        services.AddScoped<GetUserCards>();
        services.AddScoped<GetCardById>();
        services.AddScoped<CreateCard>();
        services.AddScoped<UpdateCard>();
        services.AddScoped<DeleteCard>();
        services.AddCardsInfrastructure(connectionString);
        return services;
    }

    public static async Task MigrateCardsDatabaseAsync(
        this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CardDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
