using DockerMultiProfileDemo.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace IntegrationTest;

public class IntegrationTests
{
    private readonly HttpClient _client;
    private readonly string _baseAddress = "http://myapp.local"; //TODO hardcoded

    public IntegrationTests()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(_baseAddress)
        };
    }

    [Fact]
    public async Task GetWeatherForecastReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/WeatherForecast");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetUnittest_ReturnsDataFromServiceAndCachesIt()
    {
        // Act: Call the endpoint for the first time (cache is empty initially)
        var response = await _client.GetAsync("/unittest");
        response.EnsureSuccessStatusCode();

        var firstResult = await response.Content.ReadFromJsonAsync<SomeEntityModel>();

        Assert.NotNull(firstResult);

        // Act: Call the endpoint again (should return cached result)
        var cachedResponse = await _client.GetAsync("/unittest");
        cachedResponse.EnsureSuccessStatusCode();

        var cachedResult = await cachedResponse.Content.ReadFromJsonAsync<SomeEntityModel>();

        Assert.NotNull(cachedResult);
        Assert.Equal(firstResult.someProp, cachedResult.someProp);
    }
}
