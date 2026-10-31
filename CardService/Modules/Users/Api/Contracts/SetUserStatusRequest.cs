using System.ComponentModel.DataAnnotations;

namespace TuyaCardFlow.Modules.Users.Api.Contracts;

public sealed class SetUserStatusRequest
{
    [Required]
    public bool? IsActive { get; init; }
}
