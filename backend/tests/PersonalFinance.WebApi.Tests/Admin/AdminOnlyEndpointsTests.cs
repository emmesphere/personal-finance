using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace PersonalFinance.WebApi.Tests.Admin;

public sealed class AdminOnlyEndpointsTests : IClassFixture<WebApplicationFactory<WebApiMarker>>
{
    private readonly HttpClient _httpClient;

    public AdminOnlyEndpointsTests(WebApplicationFactory<WebApiMarker> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetAdminSummary_ShouldReturnForbidden_ForNonAdminUser()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var registerResponse = await _httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Regular User",
            username = $"regular-{suffix}",
            email = $"regular-{suffix}@example.com",
            phoneNumber = (string?)null,
            password = "supersecret1",
        });
        registerResponse.EnsureSuccessStatusCode();

        var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new { username = $"regular-{suffix}", password = "supersecret1" });
        loginResponse.EnsureSuccessStatusCode();
        using var loginBody = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginBody.RootElement.GetProperty("accessToken").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/admin/summary", UriKind.Relative));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAdminSummary_ShouldReturnOk_ForSeededAdminUser()
    {
        var loginResponse = await _httpClient.PostAsJsonAsync("/api/auth/login", new { username = "admin", password = "ChangeMe123!" });
        loginResponse.EnsureSuccessStatusCode();
        using var loginBody = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var token = loginBody.RootElement.GetProperty("accessToken").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/admin/summary", UriKind.Relative));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("totalUsers").GetInt32().ShouldBeGreaterThan(0);
    }
}
