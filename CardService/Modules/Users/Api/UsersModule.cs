using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TuyaCardFlow.Modules.Users.Api.Controllers;
using TuyaCardFlow.Modules.Users.Api.Errors;
using TuyaCardFlow.Modules.Users.Application.UseCases;
using TuyaCardFlow.Modules.Users.Infrastructure;
using TuyaCardFlow.Modules.Users.Infrastructure.Persistence;

namespace TuyaCardFlow.Modules.Users.Api;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(
        this IServiceCollection services, string connectionString)
    {
        services.AddControllers().AddApplicationPart(typeof(UsersController).Assembly);
        services.AddExceptionHandler<UsersExceptionHandler>();
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.AddScoped<CreateUser>();
        services.AddScoped<GetUserById>();
        services.AddScoped<GetUsers>();
        services.AddScoped<UpdateUser>();
        services.AddScoped<SetUserStatus>();
        services.AddScoped<LoginUser>();
        services.AddScoped<ValidateUserSession>();
        services.AddUsersInfrastructure(connectionString);
        return services;
    }

    public static async Task MigrateUsersDatabaseAsync(
        this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
