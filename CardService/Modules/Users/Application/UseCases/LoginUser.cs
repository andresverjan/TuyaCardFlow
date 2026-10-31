using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Application.UseCases;

public sealed record LoginUserCommand(string Email, string Password);
public sealed record UserLoginResult(UserResponse User, Guid AuthenticationVersion);

public sealed class LoginUser(IUserRepository repository, IUserPasswordService passwords, TimeProvider clock)
{
    public async Task<UserLoginResult> ExecuteAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(command.Password) || command.Password.Length > UserPassword.MaximumLength)
            throw new InvalidUserCredentialsException();
        var email = UserProfile.Create("Acceso", command.Email).NormalizedEmail;
        var user = await repository.GetByEmailAsync(email, cancellationToken);
        // The adapter verifies a dummy hash when no credentials exist, keeping the expensive work comparable.
        var check = passwords.Verify(user?.PasswordHash, command.Password);
        if (user is null || !user.IsActive || user.PasswordHash is null || !check.Succeeded)
            throw new InvalidUserCredentialsException();
        if (check.NeedsRehash)
        {
            user.SetPasswordHash(passwords.Hash(command.Password), clock.GetUtcNow());
            await repository.SaveChangesAsync(cancellationToken);
        }
        return new UserLoginResult(UserResponse.From(user), user.AuthenticationVersion);
    }
}
