using DockerMultiProfileDemo.Database;
using DockerMultiProfileDemo.Extensions;
using DockerMultiProfileDemo.Services;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsEnvironment("Release"))
{
    builder.SetupOpenTelemetry();
}

builder.SetupDatabase();

var app = builder.Build();

app.ApplyMigrations();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (IDistributedCache cache, ILogger<Program> logger) =>
{
    var cacheKey = "weatherForecast";
    var cachedForecast = await cache.GetStringAsync(cacheKey);

    if (cachedForecast != null)
    {
        logger.LogInformation("Cache hit for key '{CacheKey}'.", cacheKey);
        return System.Text.Json.JsonSerializer.Deserialize<WeatherForecast[]>(cachedForecast);
    }

    logger.LogInformation("Cache miss for key '{CacheKey}'. Fetching data from source.", cacheKey);

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    var serializedForecast = System.Text.Json.JsonSerializer.Serialize(forecast);
    await cache.SetStringAsync(cacheKey, serializedForecast, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    });

    logger.LogInformation("Data fetched and cached with key '{CacheKey}'.", cacheKey);

    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapGet("/unittest", async (DbService service, IDistributedCache cache, ILogger<Program> logger) =>
{
    var cacheKey = "unitTestResult";

    // Try to get the cached result
    var cachedResult = await cache.GetStringAsync(cacheKey);

    if (cachedResult != null)
    {
        // Deserialize the cached JSON string back to the entity model
        var cachedEntity = System.Text.Json.JsonSerializer.Deserialize<SomeEntityModel>(cachedResult);
        return Results.Ok(cachedEntity);
    }

    logger.LogInformation("Cache hit for key '{CacheKey}'.", cacheKey);

    // Get the result from the service
    var result = await service.ReturnFirst();

    // Serialize the result to a JSON string
    var serializedResult = System.Text.Json.JsonSerializer.Serialize(result);

    // Store the serialized result in the cache
    await cache.SetStringAsync(cacheKey, serializedResult, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    });

    logger.LogInformation("Data fetched from service and cached with key '{CacheKey}'.", cacheKey);

    return Results.Ok(result);
})
.WithName("UnitTest")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}