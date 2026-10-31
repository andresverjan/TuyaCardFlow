namespace TuyaCardFlow.Modules.Users.Domain;

public static class UserPassword
{
    public const int MinimumLength = 12;
    public const int MaximumLength = 128;

    public static void Validate(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length is < MinimumLength or > MaximumLength)
            throw new UserValidationException("La contraseña debe tener entre 12 y 128 caracteres.");
    }
}
