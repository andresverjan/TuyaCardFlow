# Tuya Card Flow

Backend modular con ASP.NET Core 8, arquitectura hexagonal dentro de cada módulo, EF Core y PostgreSQL. Por ahora contiene únicamente el módulo **Cards**.

## Estructura y dependencias

```text
CardService/                              Servicio de backend
  Api/                                    Host de ASP.NET Core
    CardService.Api.csproj
    Program.cs                            Configuración global y registro de módulos
    appsettings.json                      Configuración de conexiones
  Modules/
    Cards/
      Domain/                             Entidad Card y sus reglas de negocio
        Cards.Domain.csproj
      Application/                        Puerto ICardRepository, comandos y CardResponse
        UseCases/                         CreateCard, GetCardById, GetUserCards, UpdateCard, DeleteCard
        Cards.Application.csproj
      Infrastructure/
        Persistence/                      DbContext, configuración y migraciones
          Repositories/                   EfCardRepository (adaptador de salida)
        Cards.Infrastructure.csproj
      Api/
        Controllers/                      CardsController (adaptador de entrada)
        Contracts/                        Solicitudes HTTP
        Errors/                           Errores propios de Cards
        CardsModule.cs                    Registro de dependencias y controladores del módulo
        Cards.http                        Ejemplos para Rider
        Cards.Api.csproj
      Tests/                              Pruebas de dominio y de integración HTTP + PostgreSQL
        Cards.Tests.csproj
```

Las referencias de proyecto apuntan hacia el núcleo:

```text
CardService.Api (host) → Cards.Api
Cards.Api         → Cards.Application + Cards.Infrastructure
Cards.Infrastructure → Cards.Application → Cards.Domain
```

`Domain` no depende de otros proyectos ni de paquetes. `Application` solo depende de `Domain`: no conoce EF Core, PostgreSQL ni ASP.NET. El controlador invoca casos de uso; los casos de uso utilizan el puerto `ICardRepository`; `EfCardRepository` implementa ese puerto.

Todo el código del backend vive dentro de `CardService/`. Cada módulo es dueño de sus capas, contratos HTTP, persistencia, migraciones y pruebas. Los siguientes módulos se agregarán como carpetas hermanas de Cards dentro de `CardService/Modules/`, con su propia estructura hexagonal. El host registra Cards mediante `AddCardsModule`; el módulo registra sus casos de uso, repositorio y controladores. `CardsExceptionHandler` procesa únicamente errores propios de Cards, permitiendo registrar manejadores para los siguientes módulos.

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
dotnet run --project CardService/Api --launch-profile http
```

Abre [Swagger](http://localhost:5143/swagger) o ejecuta las solicitudes de `CardService/Modules/Cards/Api/Cards.http` en Rider, empezando por POST. Ese archivo guarda automáticamente el ID creado para las solicitudes siguientes.

Si `dotnet` no está en el PATH de macOS y tienes la instalación estándar:

```bash
export PATH="/usr/local/share/dotnet:$PATH"
```

PostgreSQL escucha en `localhost:5433`, base `tuya_cards`, usuario `tuya` y contraseña local `tuya_local`. Estas credenciales son únicamente para desarrollo. El volumen `cards_postgres_data` conserva los datos al detener el contenedor con `docker compose down`.

Para usar otro servidor, configura `ConnectionStrings__Cards` con su conexión PostgreSQL. Las variables de entorno reemplazan los valores de `appsettings.json`.

## Endpoints

| Método | Ruta | Resultado |
| --- | --- | --- |
| POST | `/api/cards` | Crea una tarjeta. `201` con cuerpo y cabecera `Location`. |
| GET | `/api/cards/{id}` | Consulta una tarjeta. `200` o `404`. |
| GET | `/api/cards/user/{userId}` | Lista las tarjetas del usuario. `200` con un arreglo, vacío si no hay tarjetas. |
| PUT | `/api/cards/{id}` | Reemplaza los datos editables. `200` o `404`. |
| DELETE | `/api/cards/{id}` | Elimina físicamente la tarjeta. `204` o `404`. |

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
dotnet ef database update --project CardService/Modules/Cards/Infrastructure --startup-project CardService/Api
```

Para agregar una migración después de modificar el modelo:

```bash
dotnet ef migrations add NombreDelCambio --project CardService/Modules/Cards/Infrastructure --startup-project CardService/Api --output-dir Persistence/Migrations
```

## Pruebas

```bash
dotnet test TuyaCardFlow.sln
```

Las pruebas de integración necesitan Docker. Testcontainers crea una base PostgreSQL temporal, aplica las migraciones reales y la elimina al terminar. No utilizan la base de desarrollo. Cubren el CRUD completo, la persistencia entre solicitudes, el filtro por usuario, las validaciones y las respuestas para tarjetas inexistentes.

Para ejecutar solo las pruebas de dominio, sin Docker:

```bash
dotnet test TuyaCardFlow.sln --filter FullyQualifiedName~TuyaCardFlow.Modules.Cards.Tests.Domain
```

## Alcance de esta primera versión

Todavía no hay autenticación ni módulo de usuarios. `userId` es una referencia externa: filtra tarjetas, pero no autoriza el acceso ni verifica que el usuario exista. La autenticación y la comprobación de pertenencia quedan para una siguiente funcionalidad antes de exponer esta API a usuarios reales.

Referencias: [proveedor Npgsql para EF Core](https://www.npgsql.org/efcore/) y [migraciones de EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying).
