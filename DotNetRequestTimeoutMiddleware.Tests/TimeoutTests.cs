using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DotNetRequestTimeoutMiddleware.Tests;

public class TimeoutTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public TimeoutTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Slow_Endpoint_Should_Timeout()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/demo/slow");

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
    }

    [Fact]
    public async Task SlowAllowed_Endpoint_Should_Succeed()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/demo/slow-allowed");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
