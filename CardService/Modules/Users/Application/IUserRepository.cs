using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Application;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string normalizedEmail, Guid? excludingUserId = null,
        CancellationToken cancellationToken = default);
    Task<UserPage> GetPageAsync(int page, int pageSize, bool? isActive,
        CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed record UserPage(IReadOnlyList<User> Items, int TotalCount);
