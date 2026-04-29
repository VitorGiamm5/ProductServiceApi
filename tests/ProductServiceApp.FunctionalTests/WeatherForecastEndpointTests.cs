using System.Net;
using FluentAssertions;
using ProductServiceApp.FunctionalTests.Support;

namespace ProductServiceApp.FunctionalTests;

public class WeatherForecastEndpointTests(ProductServiceHttpClientFixture fixture)
    : IClassFixture<ProductServiceHttpClientFixture>
{
    [Fact]
    public async Task Get_WeatherForecast_Should_Return_Success()
    {
        using var response = await fixture.Client.GetAsync("/api/v1/WeatherForecast");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
