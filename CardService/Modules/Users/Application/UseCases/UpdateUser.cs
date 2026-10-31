using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Application.UseCases;

public sealed class UpdateUser(IUserRepository repository, TimeProvider timeProvider)
{
    public async Task<UserResponse> ExecuteAsync(
        Guid id, UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new UserValidationException("El usuario es obligatorio.");

        var profile = UserProfile.Create(command.FullName, command.Email);
        var user = await repository.GetByIdAsync(id, cancellationToken) ?? throw new UserNotFoundException(id);
        if (await repository.EmailExistsAsync(profile.NormalizedEmail, id, cancellationToken))
            throw new DuplicateUserEmailException();

        user.UpdateProfile(profile, timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return UserResponse.From(user);
    }
}
