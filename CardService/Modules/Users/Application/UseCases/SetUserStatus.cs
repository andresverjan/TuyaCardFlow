using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Application.UseCases;

public sealed class SetUserStatus(IUserRepository repository, TimeProvider timeProvider)
{
    public async Task<UserResponse> ExecuteAsync(
        Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new UserValidationException("El usuario es obligatorio.");

        var user = await repository.GetByIdAsync(id, cancellationToken) ?? throw new UserNotFoundException(id);
        user.SetActive(isActive, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return UserResponse.From(user);
    }
}
