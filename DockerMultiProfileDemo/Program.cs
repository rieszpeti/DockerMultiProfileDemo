using DockerMultiProfileDemo.Database;
using DockerMultiProfileDemo.Extensions;
using DockerMultiProfileDemo.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dbConnectionStringKey = "DefaultDb";
var cacheConnectionStringKey = "DefaultCache";

if (Environment.GetEnvironmentVariable("ISDOCKERENV") == "true")
{
    dbConnectionStringKey = "DockerDb";
}

var connectionString = builder.Configuration.GetConnectionString(dbConnectionStringKey);
builder.Services.AddNpgsql<AppDbContext>(connectionString);
builder.Services.AddStackExchangeRedisCache(options =>
               options.Configuration = builder.Configuration.GetConnectionString(cacheConnectionStringKey));
builder.Services.AddScoped<DbService>();

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

app.MapGet("/weatherforecast", async (IDistributedCache cache) =>
{
    var cacheKey = "weatherForecast";
    var cachedForecast = await cache.GetStringAsync(cacheKey);

    if (cachedForecast != null)
    {
        return System.Text.Json.JsonSerializer.Deserialize<WeatherForecast[]>(cachedForecast);
    }

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

    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapGet("/unittest", async (DbService service, IDistributedCache cache) =>
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

    // Get the result from the service
    var result = await service.ReturnFirst();

    // Serialize the result to a JSON string
    var serializedResult = System.Text.Json.JsonSerializer.Serialize(result);

    // Store the serialized result in the cache
    await cache.SetStringAsync(cacheKey, serializedResult, new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    });

    return Results.Ok(result);
})
.WithName("UnitTest")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Make the implicit Program class public so test projects can access it
public partial class Program;