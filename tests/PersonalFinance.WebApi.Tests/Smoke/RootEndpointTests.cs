using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;

using Shouldly;

namespace PersonalFinance.WebApi.Tests.Smoke;
public sealed class RootEndpointTests : IClassFixture<WebApplicationFactory<WebApiMarker>>
{
    private readonly HttpClient _httpClient;

    public RootEndpointTests(WebApplicationFactory<WebApiMarker> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetRoot_ShouldReturnOk()
    {
        var uri = new Uri("/", UriKind.Relative);
        var response = await _httpClient.GetAsync(uri);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("\"status\":\"ok\"");
    }

}
