using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Application.UseCases;

public sealed class CreateUser(IUserRepository repository, TimeProvider timeProvider, IUserPasswordService passwords)
{
    public async Task<UserResponse> ExecuteAsync(
        CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = User.Create(UserProfile.Create(command.FullName, command.Email), timeProvider.GetUtcNow());
        if (command.Password is not null)
            UserPassword.Validate(command.Password);
        if (await repository.EmailExistsAsync(user.NormalizedEmail, cancellationToken: cancellationToken))
            throw new DuplicateUserEmailException();

        if (command.Password is not null)
            user.SetPasswordHash(passwords.Hash(command.Password), timeProvider.GetUtcNow());

        await repository.AddAsync(user, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return UserResponse.From(user);
    }
}
