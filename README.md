# Tuya Card Flow
Backend modular con ASP.NET Core 8 y PostgreSQL, FRONT app web React y Material UI.

### Requisitos previos
* **Docker Desktop** (para la base de datos PostgreSQL).
* **.NET 8 SDK**.
* **Node.js 20+** y **npm**.
---

### Paso 1: Levantar PostgreSQL
En la raíz del proyecto, ejecuta:
```bash
docker compose up -d --wait
```

### Paso 2: Iniciar Backend
> Se puede iniciar tanto por HTTP como por HTTPS (si entra por HTTP se redirige automáticamente).
> Puedes iniciarlo por comandos o directamente en Visual Studio / Rider ejecutando **CardService.Api**:
```bash
dotnet run --project CardService/Api --launch-profile https
```

### Paso 3: Iniciar Frontend
Puedes iniciarlo con Vite mediante consola, o ejecutando **CardServiceFront** desde Visual Studio / Rider:
```bash
cd CardServiceFront/ClientApp
npm install
npm run dev
```

### Flujo y datos de prueba
Al iniciar la app por primera vez, se debe ingresar a **Crear perfil** para registrar un usuario:
* Si usaste Vite (`npm run dev`): [http://localhost:5173/crear-perfil](http://localhost:5173/crear-perfil) (o `/ingresar`)
* Si usaste el host .NET (`CardServiceFront`): [http://localhost:5214/ingresar](http://localhost:5214/ingresar)
---

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

## Ejecutar en desarrollo
Requisitos: SDK .NET 8 compatible con `global.json` y Docker con Compose. Ejecuta desde la raíz:
```bash
docker compose up -d --wait
dotnet restore
Database__ApplyMigrationsOnStartup=true dotnet run --project CardService/Api --launch-profile https
```

El API se sirve por HTTPS en `https://localhost:7198` y redirige las solicitudes HTTP de `localhost:5143`. Swagger está en [localhost:7198/swagger](https://localhost:7198/swagger). Las migraciones automáticas vienen desactivadas por defecto. Para aplicarlas en desarrollo, inicia con `Database__ApplyMigrationsOnStartup=true`; o ejecuta `dotnet tool restore` y `dotnet ef database update` para cada contexto como se muestra más abajo.

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
Las pruebas de Users y Payments son exclusivamente unitarias y no necesitan servicios externos:
```bash
dotnet test CardService/Modules/Users/Tests/Users.Tests.csproj
dotnet test CardService/Modules/Payments/Tests/Payments.Tests.csproj
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
