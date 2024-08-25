using DockerMultiProfileDemo.Database;
using DockerMultiProfileDemo.Services;
using Microsoft.EntityFrameworkCore;

namespace DockerMultiProfileDemo.Extensions
{
    internal static class DatabaseExtension
    {
        public static void SetupDatabase(this WebApplicationBuilder builder)
        {
            var dbConnectionStringKey = "DefaultDb";
            var cacheConnectionStringKey = "DefaultCache";

            if (Environment.GetEnvironmentVariable("ISDOCKERENV") == "true")
            {
                dbConnectionStringKey = "DockerDb";
                cacheConnectionStringKey = "DockerCache";
            }

            var connectionString = builder.Configuration.GetConnectionString(dbConnectionStringKey);
            builder.Services.AddNpgsql<AppDbContext>(connectionString);
            builder.Services.AddStackExchangeRedisCache(options =>
                           options.Configuration = builder.Configuration.GetConnectionString(cacheConnectionStringKey));
            builder.Services.AddScoped<DbService>();
        }

        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app?.ApplicationServices.CreateScope();

            using var dbContext = scope?.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext?.Database.Migrate();
        }
    }
}
