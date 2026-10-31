using System.ComponentModel.DataAnnotations;
using TuyaCardFlow.Modules.Users.Application;

namespace TuyaCardFlow.Modules.Users.Api.Contracts;

public sealed class UpdateUserRequest
{
    [Required, StringLength(150)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    public UpdateUserCommand ToCommand() => new(FullName, Email);
}
