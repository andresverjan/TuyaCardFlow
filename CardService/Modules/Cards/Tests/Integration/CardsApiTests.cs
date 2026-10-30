using System.Net;
using System.Net.Http.Json;
using TuyaCardFlow.Modules.Cards.Application;
using Microsoft.AspNetCore.Mvc;

namespace TuyaCardFlow.Modules.Cards.Tests.Integration;

public sealed class CardsApiTests(CardsApiFactory factory) : IClassFixture<CardsApiFactory>
{
    [Fact]
    public async Task Crud_persists_changes_between_requests_and_returns_expected_statuses()
    {
        using var client = factory.CreateApiClient();
        var userId = Guid.NewGuid();
        var createResponse = await client.PostAsJsonAsync("/api/cards", ValidCard(userId));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = (await createResponse.Content.ReadFromJsonAsync<CardResponse>())!;
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(userId, created.UserId);
        Assert.Equal("Ana Pérez", created.HolderName);
        Assert.Equal("1234", created.Last4);
        Assert.Equal($"/api/cards/{created.Id}", createResponse.Headers.Location!.AbsolutePath);

        var read = await client.GetFromJsonAsync<CardResponse>(createResponse.Headers.Location);
        Assert.Equal(created, read);

        var updateResponse = await client.PutAsJsonAsync($"/api/cards/{created.Id}", new
        {
            holderName = "Ana María Pérez", brand = "Tuya", last4 = "9876",
            expirationMonth = 10, expirationYear = 2032
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await client.GetFromJsonAsync<CardResponse>($"/api/cards/{created.Id}");
        Assert.Equal("9876", updated!.Last4);
        Assert.Equal("Ana María Pérez", updated.HolderName);
        Assert.Equal(userId, updated.UserId);

        var list = await client.GetFromJsonAsync<CardResponse[]>($"/api/cards/user/{userId}");
        Assert.Equal(updated, Assert.Single(list!));

        var deleteResponse = await client.DeleteAsync($"/api/cards/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/cards/{created.Id}")).StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<CardResponse[]>($"/api/cards/user/{userId}"))!);
    }

    [Fact]
    public async Task User_query_returns_only_that_users_cards_and_an_empty_array_when_none_exist()
    {
        using var client = factory.CreateApiClient();
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        (await client.PostAsJsonAsync("/api/cards", ValidCard(ownerId))).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync("/api/cards", ValidCard(ownerId))).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync("/api/cards", ValidCard(otherId))).EnsureSuccessStatusCode();

        var cards = await client.GetFromJsonAsync<CardResponse[]>($"/api/cards/user/{ownerId}");
        Assert.Equal(2, cards!.Length);
        Assert.All(cards, card => Assert.Equal(ownerId, card.UserId));
        Assert.Empty((await client.GetFromJsonAsync<CardResponse[]>($"/api/cards/user/{Guid.NewGuid()}"))!);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    public async Task Unknown_card_returns_problem_details(string method)
    {
        using var client = factory.CreateApiClient();
        using var request = new HttpRequestMessage(new HttpMethod(method), $"/api/cards/{Guid.NewGuid()}");
        if (method == "PUT")
            request.Content = JsonContent.Create(ValidCard(Guid.NewGuid()));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(404, problem!.Status);
        Assert.Equal("Tarjeta no encontrada", problem.Title);
    }

    [Theory]
    [InlineData("1234567890123456", 12)]
    [InlineData("12ab", 12)]
    [InlineData("1234", 0)]
    [InlineData("1234", 13)]
    public async Task Invalid_card_returns_400_and_does_not_persist(string last4, int month)
    {
        using var client = factory.CreateApiClient();
        var userId = Guid.NewGuid();
        var response = await client.PostAsJsonAsync("/api/cards", new
        {
            userId, holderName = "Ana Pérez", brand = "Tuya", last4,
            expirationMonth = month, expirationYear = 2030
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotEmpty(problem!.Errors);
        Assert.Empty((await client.GetFromJsonAsync<CardResponse[]>($"/api/cards/user/{userId}"))!);
    }

    [Fact]
    public async Task Missing_required_fields_return_validation_errors()
    {
        using var client = factory.CreateApiClient();
        var response = await client.PostAsJsonAsync("/api/cards", new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Contains("UserId", problem!.Errors.Keys);
        Assert.Contains("ExpirationMonth", problem.Errors.Keys);
        Assert.Contains("ExpirationYear", problem.Errors.Keys);
    }

    [Fact]
    public async Task Empty_user_id_returns_400_for_creation_and_listing()
    {
        using var client = factory.CreateApiClient();
        var create = await client.PostAsJsonAsync("/api/cards", ValidCard(Guid.Empty));
        var list = await client.GetAsync($"/api/cards/user/{Guid.Empty}");

        Assert.Equal(HttpStatusCode.BadRequest, create.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, list.StatusCode);
        Assert.Equal("Datos de tarjeta inválidos",
            (await create.Content.ReadFromJsonAsync<ProblemDetails>())!.Title);
    }

    private static object ValidCard(Guid userId) => new
    {
        userId, holderName = "Ana Pérez", brand = "Tuya", last4 = "1234",
        expirationMonth = 12, expirationYear = 2030
    };
}
