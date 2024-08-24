using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTest;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _domainName;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;

        _domainName = "http://myapp.local"; // TODO hardcoded
    }

    [Fact]
    public async Task GetWeatherForecastReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri(_domainName)
        });

        // Act
        var response = await client.GetAsync("WeatherForecast");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}
