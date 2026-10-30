using TuyaCardFlow.Modules.Cards.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("Cards")
    ?? throw new InvalidOperationException("La conexión 'Cards' no está configurada.");
builder.Services.AddCardsModule(connectionString);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    {
        await app.Services.MigrateCardsDatabaseAsync();
    }
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// Allows WebApplicationFactory to exercise the actual HTTP pipeline in integration tests.
public partial class Program { }
