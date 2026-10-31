namespace TuyaCardFlow.Modules.Users.Application;

public sealed class InvalidUserCredentialsException()
    : Exception("El correo o la contraseña no son correctos.");
