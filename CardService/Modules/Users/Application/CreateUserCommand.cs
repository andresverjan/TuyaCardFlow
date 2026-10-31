namespace TuyaCardFlow.Modules.Users.Application;

public sealed record CreateUserCommand(string FullName, string Email, string? Password = null);
