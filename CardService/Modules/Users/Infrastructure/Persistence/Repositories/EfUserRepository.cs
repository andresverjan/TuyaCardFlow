using Microsoft.EntityFrameworkCore;
using Npgsql;
using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Domain;
using TuyaCardFlow.Modules.Users.Infrastructure.Persistence.Configurations;

namespace TuyaCardFlow.Modules.Users.Infrastructure.Persistence.Repositories;

public sealed class EfUserRepository(UserDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        dbContext.Users.SingleOrDefaultAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<bool> EmailExistsAsync(string normalizedEmail, Guid? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users.Where(user => user.NormalizedEmail == normalizedEmail);
        if (excludingUserId.HasValue)
            query = query.Where(user => user.Id != excludingUserId.Value);
        return query.AnyAsync(cancellationToken);
    }

    public async Task<UserPage> GetPageAsync(int page, int pageSize, bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users.AsNoTracking();
        if (isActive.HasValue)
            query = query.Where(user => user.IsActive == isActive.Value);
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.OrderByDescending(user => user.CreatedAtUtc).ThenByDescending(user => user.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new UserPage(users, totalCount);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await dbContext.Users.AddAsync(user, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException postgres
            && postgres.SqlState == PostgresErrorCodes.UniqueViolation
            && postgres.ConstraintName == UserConfiguration.EmailUniqueIndex)
        {
            throw new DuplicateUserEmailException();
        }
    }
}
