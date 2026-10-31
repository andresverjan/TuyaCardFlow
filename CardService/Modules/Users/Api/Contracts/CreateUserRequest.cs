using System.ComponentModel.DataAnnotations;
using TuyaCardFlow.Modules.Users.Application;

namespace TuyaCardFlow.Modules.Users.Api.Contracts;

public sealed class CreateUserRequest
{
    [Required, StringLength(150)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [StringLength(128, MinimumLength = 12)]
    public string? Password { get; init; }

    public CreateUserCommand ToCommand() => new(FullName, Email, Password);
}
