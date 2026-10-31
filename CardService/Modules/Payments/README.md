# Payments

Registra pagos asociados a tarjetas y permite consultar el historial y el detalle de las transacciones del usuario. Los registros son inmutables: no se exponen operaciones de edición ni eliminación.

## Estructura hexagonal

```text
Payments/
  Domain/          Payment y sus validaciones
  Application/     Puertos, comandos, respuestas y casos de uso
  Infrastructure/  PostgreSQL, EF Core y adaptador hacia Cards
  Api/             PaymentsController, TransactionsController y registro del módulo
  Tests/           Pruebas unitarias de dominio y aplicación
```

`Domain` no tiene dependencias externas. `Application` solo depende de `Domain`. `Infrastructure` implementa `IPaymentRepository` y `IPaymentCardVerifier`; este último consulta el caso de uso público `GetCardById` de Cards, sin consultar directamente su DbContext ni sus tablas. La API compone las dependencias mediante `AddPaymentsModule`.

## Funcionalidad

Cada pago registrado es un movimiento del historial. `Transactions` es la API de consultas dentro de Payments y utiliza el mismo registro persistido, por lo que no hay una segunda escritura para crear un historial duplicado.

| Método | Ruta | Resultado |
| --- | --- | --- |
| POST | `/api/payments` | Registra un pago. `201` con el movimiento creado y `Location` hacia su detalle. |
| GET | `/api/users/{userId}/transactions?page=1&pageSize=20` | Historial paginado del usuario. `200`, con `items` vacío si no hay movimientos. |
| GET | `/api/users/{userId}/transactions/{id}` | Detalle del movimiento para ese usuario. `200` o `404`. |

Ejemplo de registro:

```json
{
  "userId": "3ad2eefa-4c98-4d7e-9d80-0688df9d212b",
  "cardId": "ID_DE_UNA_TARJETA_DEL_USUARIO",
  "amount": 12500.50,
  "currency": "COP",
  "description": "Pago registrado de tarjeta"
}
```

El ID de la tarjeta debe ser un UUID real creado previamente con Cards. El registro devuelve `id`, `userId`, `cardId`, `amount`, `currency`, `description` y `occurredAtUtc`. El ID y la fecha UTC se generan en el servidor.

El historial devuelve `page`, `pageSize`, `totalCount` e `items`. Se ordena por fecha descendente y después por ID descendente para mantener un orden estable. La página comienza en 1; el tamaño predeterminado es 20 y su máximo es 100.

## Reglas

- Usuario y tarjeta deben tener IDs válidos y la tarjeta debe pertenecer al usuario indicado.
- El monto es positivo, admite hasta dos decimales y tiene un máximo de `9999999999999999.99`. Los montos con más decimales se rechazan; no se redondean.
- La moneda contiene tres letras ASCII y se guarda en mayúsculas. Esta primera versión valida su formato, sin un catálogo de monedas.
- La descripción es opcional y admite hasta 250 caracteres. El texto vacío se guarda como `null`.
- Los datos inválidos devuelven `400`; una tarjeta que no existe para el usuario, o un detalle que no corresponde a ese usuario, devuelve `404`. Los errores usan `ProblemDetails`.

Esta versión registra datos de pagos; no integra una pasarela, no ejecuta cobros ni modifica saldos. El historial contiene los pagos registrados en este módulo. La API requiere sesión y valida que el `userId` corresponda al usuario autenticado o a un administrador. La tarjeta también debe pertenecer a ese usuario.

La referencia a la tarjeta es un ID externo al módulo. No hay eliminación en cascada: eliminar una tarjeta en Cards conserva los registros históricos de Payments.

## Persistencia y migraciones

Payments tiene `PaymentDbContext`, tabla `Payments` e historial de migraciones propio `__PaymentsMigrationsHistory`, separado del de Cards. Usa `ConnectionStrings:Payments` cuando está configurada; en desarrollo hereda `ConnectionStrings:Cards` si no se especifica otra conexión.

En Development, el host aplica la migración de Payments después de la de Cards si `Database:ApplyMigrationsOnStartup` está activado. PostgreSQL debe estar iniciado antes de arrancar el host.

Desde la raíz del repositorio:

```bash
dotnet tool restore
dotnet ef database update --project CardService/Modules/Payments/Infrastructure --startup-project CardService/Api --context PaymentDbContext
```

Para agregar una migración:

```bash
dotnet ef migrations add NombreDelCambio --project CardService/Modules/Payments/Infrastructure --startup-project CardService/Api --context PaymentDbContext --output-dir Persistence/Migrations
```

## Pruebas unitarias

```bash
dotnet test CardService/Modules/Payments/Tests/Payments.Tests.csproj
```

El proyecto de pruebas solo referencia Application y sus dependencias de dominio. Usa dobles en memoria y un reloj fijo: no contiene pruebas de integración, no inicia HTTP y no necesita PostgreSQL ni Docker. Cubre validaciones de dinero, pertenencia de la tarjeta, registro, fecha del servidor, errores de guardado, cancelación, detalle acotado al usuario y paginación del historial.

Las solicitudes de `Api/Payments.http` son ejemplos manuales para Rider.
