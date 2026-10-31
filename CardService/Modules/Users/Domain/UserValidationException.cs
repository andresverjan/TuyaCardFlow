namespace TuyaCardFlow.Modules.Users.Domain;

public sealed class UserValidationException(string message) : Exception(message);
