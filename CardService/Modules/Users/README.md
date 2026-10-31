# Users

Gestiona perfiles de usuario con nombre, correo y estado, además de registro, inicio y cierre de sesión. Sigue la arquitectura hexagonal de Cards y Payments.

## Estructura

```text
Users/
  Domain/          User, perfil, contraseña e invariantes
  Application/     Puertos, comandos, respuestas y casos de uso
  Infrastructure/  EF Core, PostgreSQL, hash de contraseña y migraciones
  Api/             UsersController, AuthenticationController y registro del módulo
  Tests/           Pruebas unitarias de dominio y aplicación
```

Las contraseñas se derivan con el hasher PBKDF2 de ASP.NET Core Identity; el texto original nunca se persiste ni se devuelve. La cookie de sesión es HTTP-only, de duración limitada y se valida contra el estado y la versión de autenticación del usuario en cada solicitud. Cambiar contraseña, correo o estado invalida las sesiones existentes.

## Registro y sesión

Todos los endpoints de la API requieren sesión salvo registro, login y emisión del token CSRF. El frontend sirve bajo el mismo origen y pasa `/api` al backend.

| Método | Ruta | Acceso | Resultado |
| --- | --- | --- | --- |
| GET | `/api/auth/csrf` | Público | Emite la cookie y el token CSRF necesario para formularios. |
| POST | `/api/users` | Público + CSRF | Crea el perfil y guarda la contraseña derivada. `201`, `400` o `409`. |
| POST | `/api/auth/login` | Público + CSRF | Valida credenciales y crea la cookie de sesión. `200` o `401`. |
| GET | `/api/auth/me` | Sesión | Devuelve el perfil de la sesión actual. |
| POST | `/api/auth/logout` | Sesión + CSRF | Cierra la sesión y vence la cookie. `204`. |

Registro recibe:

```json
{
  "fullName": "Ana Pérez",
  "email": "ana@example.com",
  "password": "una frase de acceso segura"
}
```

Para llamadas HTTP manuales, primero solicita `GET /api/auth/csrf`, conserva las cookies y envía el `token` de la respuesta en la cabecera `X-CSRF-TOKEN` al registrar o iniciar sesión. El backend aplica límites por dirección IP: 10 registros y 8 intentos de login por minuto.

Las credenciales inválidas, perfiles inactivos o perfiles sin contraseña reciben la misma respuesta `401`. No se revela si un correo existe. Los intentos de autenticación reciben respuestas `Cache-Control: no-store`.

## Perfiles

| Método | Ruta | Acceso | Resultado |
| --- | --- | --- | --- |
| GET | `/api/users?page=1&pageSize=20&isActive=true` | Administrador | Lista paginada. |
| GET | `/api/users/{id}` | Propietario o administrador | Consulta un perfil. `200` o `404`. |
| PUT | `/api/users/{id}` | Propietario o administrador | Actualiza nombre y correo. `200`, `400`, `404` o `409`. |
| DELETE | `/api/users/{id}` | Propietario o administrador | Baja lógica; también invalida la sesión. `204`. |
| PATCH | `/api/users/{id}/status` | Administrador | Activa o desactiva el perfil. |

El propietario solo puede consultar y actualizar su perfil. La lista y los cambios de estado son operaciones administrativas. Para habilitar un administrador, configura `UsersAuthentication__AdministratorUserIds` con UUIDs de usuarios conocidos. Por defecto nadie tiene rol administrativo.

## Reglas

- Nombre obligatorio, máximo 150 caracteres.
- Correo obligatorio, hasta 254 caracteres, normalizado a minúsculas y único sin distinguir mayúsculas, también entre usuarios inactivos.
- Contraseña obligatoria para el registro desde la app, mínimo 12 y máximo 128 caracteres. Un perfil legado creado sin contraseña no puede iniciar sesión hasta que se le asigne una mediante un flujo de recuperación, todavía pendiente.
- Desactivar un perfil invalida la sesión en la siguiente solicitud autenticada. Reactivarlo no restaura la cookie anterior.
- La identidad y estado del usuario se consultan en PostgreSQL; la autenticación no depende de tokens guardados por el navegador.

## Persistencia y migraciones

Users tiene su `UserDbContext`, tabla `Users` e historial `__UsersMigrationsHistory`. Comparte `ConnectionStrings:Cards` salvo que se configure `ConnectionStrings:Users`.

En `Development`, el backend aplica migraciones al iniciar cuando `Database:ApplyMigrationsOnStartup` está activo. Para aplicar manualmente la migración de autenticación:

```bash
dotnet tool restore
dotnet ef database update --project CardService/Modules/Users/Infrastructure --startup-project CardService/Api --context UserDbContext
```

## Pruebas unitarias

```bash
dotnet test CardService/Modules/Users/Tests/Users.Tests.csproj
```

Las pruebas de Users usan repositorios y servicio de contraseñas simulados. No arrancan HTTP, EF Core, PostgreSQL ni Docker. Cubren reglas de perfil, hashing a través del puerto de aplicación, login, credenciales inválidas, rehash y revocación de sesiones.
