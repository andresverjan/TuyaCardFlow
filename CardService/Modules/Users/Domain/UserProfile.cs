using System.Net.Mail;

namespace TuyaCardFlow.Modules.Users.Domain;

public sealed record UserProfile
{
    public const int MaximumNameLength = 150;
    public const int MaximumEmailLength = 254;

    public string FullName { get; }
    public string Email { get; }
    public string NormalizedEmail { get; }

    private UserProfile(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
        NormalizedEmail = email.ToUpperInvariant();
    }

    public static UserProfile Create(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > MaximumNameLength
            || fullName.Any(char.IsControl))
            throw new UserValidationException("El nombre es obligatorio, admite hasta 150 caracteres y no puede contener caracteres de control.");

        var address = email?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(address) || address.Length > MaximumEmailLength
            || address.Any(character => char.IsWhiteSpace(character) || char.IsControl(character))
            || !MailAddress.TryCreate(address, out var parsed)
            || parsed.Address != address || parsed.DisplayName.Length != 0)
            throw new UserValidationException("El correo electrónico debe ser una dirección válida y admitir hasta 254 caracteres.");

        return new UserProfile(fullName.Trim(), address);
    }
}
