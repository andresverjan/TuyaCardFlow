using TuyaCardFlow.Modules.Cards.Api;
using TuyaCardFlow.Modules.Payments.Api;
using TuyaCardFlow.Modules.Users.Api;
using TuyaCardFlow.Modules.Users.Api.Authentication;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);
builder.Services.AddUserAuthentication(builder.Configuration, builder.Environment.IsDevelopment());

var connectionString = builder.Configuration.GetConnectionString("Cards")
    ?? throw new InvalidOperationException("La conexión 'Cards' no está configurada.");
builder.Services.AddUsersModule(builder.Configuration.GetConnectionString("Users") ?? connectionString);
builder.Services.AddCardsModule(connectionString);
builder.Services.AddPaymentsModule(builder.Configuration.GetConnectionString("Payments") ?? connectionString);

var app = builder.Build();

app.UseExceptionHandler();
app.UseForwardedHeaders();
app.UseStatusCodePages();
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
        context.Response.Headers.CacheControl = "no-store";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    {
        await app.Services.MigrateUsersDatabaseAsync();
        await app.Services.MigrateCardsDatabaseAsync();
        await app.Services.MigratePaymentsDatabaseAsync();
    }
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.Run();

// Allows WebApplicationFactory to exercise the actual HTTP pipeline in integration tests.
public partial class Program { }
