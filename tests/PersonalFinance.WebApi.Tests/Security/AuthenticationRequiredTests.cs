using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace PersonalFinance.WebApi.Tests.Security;

public sealed class AuthenticationRequiredTests : IClassFixture<WebApplicationFactory<WebApiMarker>>
{
    private readonly HttpClient _httpClient;

    public AuthenticationRequiredTests(WebApplicationFactory<WebApiMarker> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task PostJournalEntry_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var payload = new
        {
            date = "2026-01-15",
            description = "test",
            lines = new object[]
            {
                new { accountId = Guid.NewGuid(), type = "Debit", amount = 10m },
                new { accountId = Guid.NewGuid(), type = "Credit", amount = 10m },
            },
        };

        var response = await _httpClient.PostAsJsonAsync($"/api/ledgers/{Guid.NewGuid()}/journal-entry/post", payload);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostJournalEntry_ShouldReachHandler_WhenValidTokenProvided()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var registerResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Spike User",
            username = $"spike-{suffix}",
            email = $"spike-{suffix}@example.com",
            phoneNumber = (string?)null,
            password = "supersecret1",
        });
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new
        {
            username = $"spike-{suffix}",
            password = "supersecret1",
        });
        loginResponse.EnsureSuccessStatusCode();

        using var loginBody = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var accessToken = loginBody.RootElement.GetProperty("accessToken").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/ledgers/{Guid.NewGuid()}/journal-entry/post")
        {
            Content = JsonContent.Create(new
            {
                date = "2026-01-15",
                description = "test",
                lines = new object[]
                {
                    new { accountId = Guid.NewGuid(), type = "Debit", amount = 10m },
                    new { accountId = Guid.NewGuid(), type = "Credit", amount = 10m },
                },
            }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        // Not 401/403: proves the JWT was validated and ICurrentUserService resolved the caller's
        // identity from the request's HttpContext inside Wolverine's InvokeAsync pipeline. The ledger
        // doesn't exist, so the handler correctly reports 404 rather than throwing or being blocked.
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
