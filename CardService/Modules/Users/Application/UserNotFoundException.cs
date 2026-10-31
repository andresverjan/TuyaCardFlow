namespace TuyaCardFlow.Modules.Users.Application;

public sealed class UserNotFoundException(Guid userId)
    : Exception($"No se encontró el usuario '{userId}'.");
