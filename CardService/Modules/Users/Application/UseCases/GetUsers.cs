using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Application.UseCases;

public sealed class GetUsers(IUserRepository repository)
{
    public async Task<UserListResponse> ExecuteAsync(
        int page = 1, int pageSize = 20, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100 || (long)(page - 1) * pageSize > int.MaxValue)
            throw new UserValidationException("La página debe ser positiva y el tamaño debe estar entre 1 y 100.");

        var result = await repository.GetPageAsync(page, pageSize, isActive, cancellationToken);
        return new UserListResponse(page, pageSize, result.TotalCount,
            result.Items.Select(UserResponse.From).ToArray());
    }
}
