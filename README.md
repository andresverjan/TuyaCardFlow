# Tuya Card Flow

Backend modular con ASP.NET Core 8 y PostgreSQL, FRONT app web React y Material UI.

## Estructura y dependencias
```text
CardService/                              Servicio de backend
  Api/                                    Host de ASP.NET Core
    CardService.Api.csproj
    Program.cs                            Configuración global y registro de módulos
    appsettings.json                      Configuración de conexiones
  Modules/                                  Módulos hexagonales del backend
    Cards/
      Domain/ Application/ Infrastructure/ Api/ Tests/
    Users/
      Domain/ Application/ Infrastructure/ Api/ Tests/
    Payments/
      Domain/ Application/ Infrastructure/ Api/ Tests/
CardServiceFront/                           Host del frontend React y Material UI
  ClientApp/                                 Módulos, páginas, estilos y pruebas
```

Las referencias de proyecto apuntan hacia el núcleo:

```text
CardService.Api (host) → Users.Api
Users.Api         → Users.Application + Users.Infrastructure
Users.Infrastructure → Users.Application → Users.Domain
CardService.Api (host) → Cards.Api
Cards.Api         → Cards.Application + Cards.Infrastructure
Cards.Infrastructure → Cards.Application → Cards.Domain
CardService.Api (host) → Payments.Api
Payments.Api      → Payments.Application + Payments.Infrastructure
Payments.Infrastructure → Payments.Application → Payments.Domain
Payments.Infrastructure → Cards.Application (adaptador de verificación de tarjetas)
```

`Domain` no depende de otros proyectos ni de paquetes. `Application` solo depende de `Domain`: no conoce EF Core, PostgreSQL ni ASP.NET. El controlador invoca casos de uso; los casos de uso utilizan el puerto `ICardRepository`; `EfCardRepository` implementa ese puerto.

Todo el código del backend vive dentro de `CardService/`. Cada módulo es dueño de sus capas, contratos HTTP, persistencia, migraciones y pruebas. Los siguientes módulos se agregarán como carpetas hermanas de Cards dentro de `CardService/Modules/`, con su propia estructura hexagonal. El host registra Cards mediante `AddCardsModule`; el módulo registra sus casos de uso, repositorio y controladores. `CardsExceptionHandler` procesa únicamente errores propios de Cards, permitiendo registrar manejadores para los siguientes módulos.

Users se registra con `AddUsersModule` y gestiona perfil, contraseña derivada, login con cookie HTTP-only y cierre de sesión. Su documentación está en [Users](CardService/Modules/Users/README.md).

Payments se registra con `AddPaymentsModule`. Permite registrar pagos y consultar movimientos del usuario, sin edición ni eliminación ni ejecución de cobros reales. Su estructura, reglas y endpoints están en [Payments](CardService/Modules/Payments/README.md).

Flujo de una consulta:

```text
GET /api/cards/user/{userId}
  → CardsController
  → GetUserCards
  → ICardRepository
  → EfCardRepository → CardDbContext → PostgreSQL
  ← CardResponse[]
```

## Ejecutar en desarrollo
Requisitos: SDK .NET 8 compatible con `global.json` y Docker con Compose. Ejecuta desde la raíz:

```bash
docker compose up -d --wait
dotnet restore
Database__ApplyMigrationsOnStartup=true dotnet run --project CardService/Api --launch-profile https
```

El API se sirve por HTTPS en `https://localhost:7198` y redirige las solicitudes HTTP de `localhost:5143`. Swagger está en [localhost:7198/swagger](https://localhost:7198/swagger). Las migraciones automáticas vienen desactivadas por defecto. Para aplicarlas en desarrollo, inicia con `Database__ApplyMigrationsOnStartup=true`; o ejecuta `dotnet tool restore` y `dotnet ef database update` para cada contexto como se muestra más abajo.

## Ver la app React

Requisitos: Node.js 22.12 o posterior y npm. Desde la raíz, inicia primero PostgreSQL y el backend siguiendo los pasos anteriores. En otra terminal inicia el frontend:

```bash
cd CardServiceFront/ClientApp
npm ci
npm run dev
```

Abre la URL que imprime Vite (por defecto [http://localhost:5173](http://localhost:5173); si ese puerto está ocupado, usará el siguiente disponible). Vite envía `/api` al backend HTTPS local en `https://localhost:7198`; puedes cambiar el destino con `API_PROXY_TARGET`. Para abrirlo mediante el host ASP.NET Core en `http://localhost:5214`, detén Vite y ejecuta desde la raíz:

```bash
dotnet run --project CardServiceFront --launch-profile http
```

El host compila React durante `dotnet build` y sirve la app junto con el proxy `/api`. Al abrir la app aparece el login en `/ingresar`; desde el enlace «Crear perfil» se llega al registro `/crear-perfil`. Después del acceso, el perfil está en `/mi-perfil`. No se requiere configuración de CORS porque el navegador usa un solo origen.

Si `dotnet` no está en el PATH de macOS y tienes la instalación estándar:
```bash
export PATH="/usr/local/share/dotnet:$PATH"
```

PostgreSQL escucha en `localhost:5433`, base `tuya_cards`, usuario `tuya` y contraseña local `tuya_local`. Estas credenciales son únicamente para desarrollo. El volumen `cards_postgres_data` conserva los datos al detener el contenedor con `docker compose down`.

Para usar otro servidor, configura `ConnectionStrings__Cards` con su conexión PostgreSQL. Las variables de entorno reemplazan los valores de `appsettings.json`. `UsersAuthentication__AdministratorUserIds` permite asignar rol administrativo a UUIDs explícitos; por defecto no hay administradores.

Users y Payments comparten esa conexión por defecto. Puedes configurar `ConnectionStrings__Users` y `ConnectionStrings__Payments` para usar otras bases de datos. Cada contexto conserva su historial de migraciones independiente.

## Endpoints
| Método | Ruta | Resultado |
| --- | --- | --- |
| GET | `/api/auth/csrf` | Obtiene token CSRF para registro y login. Público. |
| POST | `/api/users` | Crea un perfil con contraseña derivada. Público + CSRF. `201`. |
| POST | `/api/auth/login` | Inicia sesión y emite cookie. Público + CSRF. `200` o `401`. |
| GET | `/api/auth/me` | Devuelve el perfil de la sesión actual. |
| POST | `/api/auth/logout` | Cierra sesión. `204`. |
| GET | `/api/users` | Lista perfiles (solo administrador). |
| GET/PUT/DELETE | `/api/users/{id}` | Perfil propio o de administrador; DELETE desactiva. |
| PATCH | `/api/users/{id}/status` | Activa o desactiva (solo administrador). |
| POST | `/api/cards` | Crea una tarjeta. `201` con cuerpo y cabecera `Location`. |
| GET | `/api/cards/{id}` | Consulta una tarjeta. `200` o `404`. |
| GET | `/api/cards/user/{userId}` | Lista las tarjetas del usuario. `200` con un arreglo, vacío si no hay tarjetas. |
| PUT | `/api/cards/{id}` | Reemplaza los datos editables. `200` o `404`. |
| DELETE | `/api/cards/{id}` | Elimina físicamente la tarjeta. `204` o `404`. |
| POST | `/api/payments` | Registra un pago de una tarjeta del usuario. `201`. |
| GET | `/api/users/{userId}/transactions` | Historial paginado de pagos registrados. `200`. |
| GET | `/api/users/{userId}/transactions/{id}` | Detalle de un movimiento del usuario. `200` o `404`. |

Ejemplo de POST:

```json
{
  "userId": "3ad2eefa-4c98-4d7e-9d80-0688df9d212b",
  "holderName": "Ana Pérez",
  "brand": "Tuya",
  "last4": "1234",
  "expirationMonth": 12,
  "expirationYear": 2030
}
```

PUT recibe los mismos campos excepto `userId`: el propietario y el ID se conservan. La respuesta agrega `id` y siempre incluye `userId`.

Los IDs son UUID. El titular admite hasta 150 caracteres y la marca hasta 50; ambos son obligatorios y se guardan sin espacios al inicio o al final. `last4` requiere exactamente cuatro dígitos ASCII, el mes va de 1 a 12 y el año de 2000 a 9999. Esta versión permite registrar tarjetas vencidas. No se exige unicidad de los últimos cuatro dígitos, ya que pueden repetirse entre tarjetas.

Los datos inválidos devuelven `400` y las tarjetas inexistentes `404`, con formato `ProblemDetails`. Los errores inesperados devuelven `500` sin detalles internos. El modelo almacena únicamente los últimos cuatro dígitos; no contiene PAN completo ni CVV.

## Migraciones

La migración inicial se encuentra en `CardService/Modules/Cards/Infrastructure/Persistence/Migrations`. En **Development** se aplica al iniciar la API si `Database:ApplyMigrationsOnStartup` es `true`. En otros entornos no se modifican esquemas al arrancar; aplica las migraciones como paso separado.

```bash
dotnet tool restore
dotnet ef database update --project CardService/Modules/Users/Infrastructure --startup-project CardService/Api --context UserDbContext
dotnet ef database update --project CardService/Modules/Cards/Infrastructure --startup-project CardService/Api --context CardDbContext
dotnet ef database update --project CardService/Modules/Payments/Infrastructure --startup-project CardService/Api --context PaymentDbContext
```

Para agregar una migración después de modificar el modelo:

```bash
dotnet ef migrations add NombreDelCambio --project CardService/Modules/Cards/Infrastructure --startup-project CardService/Api --context CardDbContext --output-dir Persistence/Migrations
```

## Pruebas
Las pruebas de Users y Payments son exclusivamente unitarias y no necesitan servicios externos. El frontend también tiene pruebas unitarias con Vitest:
```bash
dotnet test CardService/Modules/Users/Tests/Users.Tests.csproj
dotnet test CardService/Modules/Payments/Tests/Payments.Tests.csproj
cd CardServiceFront/ClientApp && npm test
```
Para ejecutar toda la solución, incluyendo las pruebas de integración existentes de Cards:

```bash
dotnet test TuyaCardFlow.sln
```
Las pruebas de integración necesitan Docker. Testcontainers crea una base PostgreSQL temporal, aplica las migraciones reales y la elimina al terminar. No utilizan la base de desarrollo. Cubren el CRUD completo, la persistencia entre solicitudes, el filtro por usuario, las validaciones y las respuestas para tarjetas inexistentes.

Para ejecutar solo las pruebas de dominio, sin Docker:

```bash
dotnet test TuyaCardFlow.sln --filter FullyQualifiedName~TuyaCardFlow.Modules.Cards.Tests.Domain
```
