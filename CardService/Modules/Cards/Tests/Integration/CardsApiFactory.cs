using TuyaCardFlow.Modules.Cards.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TuyaCardFlow.Modules.Cards.Tests.Integration;

public sealed class CardsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer database = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Replace the registration, since Program has already read its connection string.
            services.RemoveAll<CardDbContext>();
            services.RemoveAll<DbContextOptions<CardDbContext>>();
            services.AddDbContext<CardDbContext>(options =>
                options.UseNpgsql(database.GetConnectionString()));
            // Existing integration tests cover Cards CRUD using an administrator test identity.
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "CardsTest";
                options.DefaultChallengeScheme = "CardsTest";
            }).AddScheme<AuthenticationSchemeOptions, CardsTestAuthenticationHandler>("CardsTest", _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<CardDbContext>();
        await context.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await database.DisposeAsync();
    }

    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false
    });
}

public sealed class CardsTestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "cfe2ed8d-d455-42da-918a-014c827be51e"),
            new Claim(ClaimTypes.Role, "Administrator")
        ], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
