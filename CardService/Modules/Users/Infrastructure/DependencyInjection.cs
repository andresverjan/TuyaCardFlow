using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Infrastructure.Persistence;
using TuyaCardFlow.Modules.Users.Infrastructure.Persistence.Repositories;
using TuyaCardFlow.Modules.Users.Infrastructure.Authentication;

namespace TuyaCardFlow.Modules.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<UserDbContext>(options => options.UseNpgsql(connectionString,
            postgres => postgres.MigrationsHistoryTable("__UsersMigrationsHistory")));
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddSingleton<IUserPasswordService, UserPasswordService>();
        return services;
    }
}
