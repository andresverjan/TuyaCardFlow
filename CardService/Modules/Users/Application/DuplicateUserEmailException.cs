namespace TuyaCardFlow.Modules.Users.Application;

public sealed class DuplicateUserEmailException()
    : Exception("Ya existe un usuario con ese correo electrónico.");
