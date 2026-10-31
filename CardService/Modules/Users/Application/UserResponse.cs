using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Application;

public sealed record UserResponse(
    Guid Id, string FullName, string Email, bool IsActive,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc)
{
    internal static UserResponse From(User user) => new(
        user.Id, user.FullName, user.Email, user.IsActive, user.CreatedAtUtc, user.UpdatedAtUtc);
}
