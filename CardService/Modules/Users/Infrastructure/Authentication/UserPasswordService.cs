using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TuyaCardFlow.Modules.Users.Application;

namespace TuyaCardFlow.Modules.Users.Infrastructure.Authentication;

public sealed class UserPasswordService : IUserPasswordService
{
    private readonly PasswordHasher<object> hasher = new(Options.Create(new PasswordHasherOptions { IterationCount = 210000 }));
    private readonly object subject = new();
    private readonly string dummyHash;

    public UserPasswordService() => dummyHash = hasher.HashPassword(subject, Guid.NewGuid().ToString("N"));
    public string Hash(string password) => hasher.HashPassword(subject, password);

    public PasswordCheck Verify(string? passwordHash, string password)
    {
        try
        {
            var result = hasher.VerifyHashedPassword(subject, passwordHash ?? dummyHash, password);
            return new PasswordCheck(passwordHash is not null && result != PasswordVerificationResult.Failed,
                result == PasswordVerificationResult.SuccessRehashNeeded);
        }
        catch (FormatException)
        {
            return new PasswordCheck(false);
        }
    }
}
