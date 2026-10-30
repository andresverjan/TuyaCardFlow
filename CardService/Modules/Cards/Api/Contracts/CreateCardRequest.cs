using System.ComponentModel.DataAnnotations;
using TuyaCardFlow.Modules.Cards.Application;

namespace TuyaCardFlow.Modules.Cards.Api.Contracts;

public sealed class CreateCardRequest
{
    [Required]
    public Guid? UserId { get; init; }

    [Required, StringLength(150)]
    public string HolderName { get; init; } = string.Empty;

    [Required, StringLength(50)]
    public string Brand { get; init; } = string.Empty;

    [Required, RegularExpression("^[0-9]{4}$")]
    public string Last4 { get; init; } = string.Empty;

    [Required, Range(1, 12)]
    public int? ExpirationMonth { get; init; }

    [Required, Range(2000, 9999)]
    public int? ExpirationYear { get; init; }

    public CreateCardCommand ToCommand() => new(
        UserId!.Value, HolderName, Brand, Last4, ExpirationMonth!.Value, ExpirationYear!.Value);
}
