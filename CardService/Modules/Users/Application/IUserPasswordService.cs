namespace TuyaCardFlow.Modules.Users.Application;

public interface IUserPasswordService
{
    string Hash(string password);
    PasswordCheck Verify(string? passwordHash, string password);
}

public readonly record struct PasswordCheck(bool Succeeded, bool NeedsRehash = false);
