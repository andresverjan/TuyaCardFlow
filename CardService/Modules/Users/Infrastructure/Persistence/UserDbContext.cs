using Microsoft.EntityFrameworkCore;
using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Infrastructure.Persistence;

public sealed class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserDbContext).Assembly);
}
