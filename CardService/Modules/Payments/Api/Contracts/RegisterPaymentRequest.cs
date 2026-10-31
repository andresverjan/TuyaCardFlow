using System.ComponentModel.DataAnnotations;
using TuyaCardFlow.Modules.Payments.Application;

namespace TuyaCardFlow.Modules.Payments.Api.Contracts;

public sealed class RegisterPaymentRequest
{
    [Required]
    public Guid? UserId { get; init; }

    [Required]
    public Guid? CardId { get; init; }

    [Required]
    public decimal? Amount { get; init; }

    [Required, RegularExpression("^[A-Za-z]{3}$")]
    public string Currency { get; init; } = string.Empty;

    [StringLength(250)]
    public string? Description { get; init; }

    public RegisterPaymentCommand ToCommand() => new(
        UserId!.Value, CardId!.Value, Amount!.Value, Currency, Description);
}
