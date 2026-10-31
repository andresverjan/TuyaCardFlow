using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Application.UseCases;

public sealed class GetUserById(IUserRepository repository)
{
    public async Task<UserResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new UserValidationException("El usuario es obligatorio.");

        var user = await repository.GetByIdAsync(id, cancellationToken) ?? throw new UserNotFoundException(id);
        return UserResponse.From(user);
    }
}
