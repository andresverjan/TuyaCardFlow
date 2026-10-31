using TuyaCardFlow.Modules.Users.Application;
using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Tests.TestDoubles;

internal sealed class StubUserRepository : IUserRepository
{
    public List<User> Existing { get; } = [];
    public List<User> Added { get; } = [];
    public List<CancellationToken> Tokens { get; } = [];
    public int ReadCalls { get; private set; }
    public int EmailChecks { get; private set; }
    public int SaveCalls { get; private set; }
    public string? QueriedEmail { get; private set; }
    public Guid? ExcludedId { get; private set; }
    public (int Page, int PageSize, bool? IsActive)? QueriedPage { get; private set; }
    public UserPage PageResult { get; set; } = new([], 0);
    public Exception? SaveFailure { get; set; }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RecordToken(cancellationToken);
        ReadCalls++;
        return Task.FromResult(Existing.SingleOrDefault(user => user.Id == id));
    }

    public Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        RecordToken(cancellationToken);
        ReadCalls++;
        return Task.FromResult(Existing.SingleOrDefault(user => user.NormalizedEmail == normalizedEmail));
    }

    public Task<bool> EmailExistsAsync(string normalizedEmail, Guid? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        RecordToken(cancellationToken);
        EmailChecks++;
        QueriedEmail = normalizedEmail;
        ExcludedId = excludingUserId;
        return Task.FromResult(Existing.Any(user =>
            user.NormalizedEmail == normalizedEmail && user.Id != excludingUserId));
    }

    public Task<UserPage> GetPageAsync(int page, int pageSize, bool? isActive,
        CancellationToken cancellationToken = default)
    {
        RecordToken(cancellationToken);
        ReadCalls++;
        QueriedPage = (page, pageSize, isActive);
        return Task.FromResult(PageResult);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        RecordToken(cancellationToken);
        Added.Add(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        RecordToken(cancellationToken);
        SaveCalls++;
        return SaveFailure is null ? Task.CompletedTask : Task.FromException(SaveFailure);
    }

    private void RecordToken(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Tokens.Add(cancellationToken);
    }
}

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal static class UserSamples
{
    public static readonly DateTimeOffset Now = new(2030, 1, 15, 12, 0, 0, TimeSpan.Zero);
    public static User Create(string email = "ana@example.com") =>
        User.Create(UserProfile.Create("Ana Pérez", email), Now);
}

internal sealed class StubUserPasswordService : IUserPasswordService
{
    public bool Valid { get; set; } = true;
    public bool NeedsRehash { get; set; }
    public int VerifyCalls { get; private set; }
    public string Hash(string password) => "test-only-opaque-hash";
    public PasswordCheck Verify(string? passwordHash, string password)
    {
        VerifyCalls++;
        return new PasswordCheck(Valid && passwordHash is not null, NeedsRehash);
    }
}
