using DockerMultiProfileDemo.Database;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Json;

namespace IntegrationTest;

public class IntegrationTests
{
    private readonly HttpClient _client;
    private readonly string _csharpAddress;
    private readonly string _pythonAddress;

    public IntegrationTests()
    {
        DotEnv.Load(".env");

        var baseAddress = Environment.GetEnvironmentVariable("DOMAIN_NAME")
                           ?? throw new ArgumentNullException("DOMAIN_NAME environment variable is not set.");

        var csharpPrefix = Environment.GetEnvironmentVariable("DOMAIN_CSHARP_PREFIX")
                           ?? throw new ArgumentNullException("DOMAIN_CSHARP_PREFIX environment variable is not set.");

        var pythonPrefix = Environment.GetEnvironmentVariable("DOMAIN_PYTHON_PREFIX")
                           ?? throw new ArgumentNullException("DOMAIN_PYTHON_PREFIX environment variable is not set.");

        _csharpAddress = $"http://{csharpPrefix}.{baseAddress}";
        _pythonAddress = $"http://{pythonPrefix}.{baseAddress}";

        _client = new HttpClient();
    }

    [Fact]
    public async Task GetWeatherForecastReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        _client.BaseAddress = new Uri(_csharpAddress);

        // Act
        var response = await _client.GetAsync("/WeatherForecast");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task GetUnittest_ReturnsDataFromServiceAndCachesIt()
    {
        // Arrange
        _client.BaseAddress = new Uri(_csharpAddress);

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

    [Fact]
    public async Task Python_HelloWorld_Endpoint_ReturnsHelloWorld()
    {
        // Arrange
        _client.BaseAddress = new Uri(_pythonAddress);

        // Act
        var response = await _client.GetAsync("/helloworld");

        // Assert
        response.EnsureSuccessStatusCode(); // Ensure we got a success status code
        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JObject.Parse(responseString);

        Assert.Equal("Hello World", responseObject["message"]?.ToString());
    }
}
