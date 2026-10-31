namespace TuyaCardFlow.Modules.Users.Domain;

public sealed class User
{
    private User() { }

    public Guid Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public Guid AuthenticationVersion { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static User Create(UserProfile profile, DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var timestamp = RequireUtc(createdAtUtc);
        var user = new User
        {
            Id = Guid.NewGuid(), IsActive = true, AuthenticationVersion = Guid.NewGuid(),
            CreatedAtUtc = timestamp, UpdatedAtUtc = timestamp
        };
        user.ApplyProfile(profile);
        return user;
    }

    public void UpdateProfile(UserProfile profile, DateTimeOffset updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var timestamp = RequireUtc(updatedAtUtc);
        if (Email != profile.Email)
            AuthenticationVersion = Guid.NewGuid();
        ApplyProfile(profile);
        UpdatedAtUtc = timestamp;
    }

    public void SetActive(bool isActive, DateTimeOffset updatedAtUtc)
    {
        var timestamp = RequireUtc(updatedAtUtc);
        if (IsActive == isActive)
            return;

        IsActive = isActive;
        AuthenticationVersion = Guid.NewGuid();
        UpdatedAtUtc = timestamp;
    }

    public void SetPasswordHash(string passwordHash, DateTimeOffset updatedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(passwordHash) || passwordHash.Length > 1024)
            throw new UserValidationException("La credencial no es válida.");
        var timestamp = RequireUtc(updatedAtUtc);
        PasswordHash = passwordHash;
        AuthenticationVersion = Guid.NewGuid();
        UpdatedAtUtc = timestamp;
    }

    private void ApplyProfile(UserProfile profile)
    {
        FullName = profile.FullName;
        Email = profile.Email;
        NormalizedEmail = profile.NormalizedEmail;
    }

    private static DateTimeOffset RequireUtc(DateTimeOffset timestamp)
    {
        if (timestamp == default)
            throw new UserValidationException("La fecha de la operación es obligatoria.");
        return timestamp.ToUniversalTime();
    }
}
