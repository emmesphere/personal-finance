using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace PersonalFinance.WebApi.Tests.Users;

public sealed class GetMeEndpointTests : IClassFixture<WebApplicationFactory<WebApiMarker>>
{
    private readonly HttpClient _httpClient;

    public GetMeEndpointTests(WebApplicationFactory<WebApiMarker> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetMe_ShouldReturnUnauthorized_WhenNoTokenProvided()
    {
        var response = await _httpClient.GetAsync(new Uri("/api/me", UriKind.Relative));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_ShouldReturnProfileAndOwnedLedger_WhenAuthenticated()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var registerResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Me Tester",
            username = $"me-{suffix}",
            email = $"me-{suffix}@example.com",
            phoneNumber = (string?)null,
            password = "supersecret1",
        });
        registerResponse.EnsureSuccessStatusCode();
        using var registerBody = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        var userId = registerBody.RootElement.GetProperty("userId").GetString();
        var ledgerId = registerBody.RootElement.GetProperty("ledgerId").GetString();

        var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new { username = $"me-{suffix}", password = "supersecret1" });
        loginResponse.EnsureSuccessStatusCode();
        using var loginBody = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginBody.RootElement.GetProperty("accessToken").GetString();

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var meResponse = await _httpClient.GetAsync(new Uri("/api/me", UriKind.Relative));
        meResponse.EnsureSuccessStatusCode();
        using var me = JsonDocument.Parse(await meResponse.Content.ReadAsStringAsync());

        me.RootElement.GetProperty("id").GetString().ShouldBe(userId);
        me.RootElement.GetProperty("username").GetString().ShouldBe($"me-{suffix}");
        me.RootElement.GetProperty("role").GetString().ShouldBe("User");
        me.RootElement.GetProperty("isActive").GetBoolean().ShouldBeTrue();

        var ledgers = me.RootElement.GetProperty("ledgers");
        ledgers.GetArrayLength().ShouldBe(1);
        ledgers[0].GetProperty("ledgerId").GetString().ShouldBe(ledgerId);
        ledgers[0].GetProperty("isOwner").GetBoolean().ShouldBeTrue();
    }
}
