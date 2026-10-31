namespace TuyaCardFlow.Modules.Users.Application.UseCases;

public sealed class ValidateUserSession(IUserRepository repository)
{
    public async Task<UserResponse?> ExecuteAsync(Guid id, Guid version, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetByIdAsync(id, cancellationToken);
        return user is { IsActive: true, PasswordHash: not null } && user.AuthenticationVersion == version
            ? UserResponse.From(user) : null;
    }
}
