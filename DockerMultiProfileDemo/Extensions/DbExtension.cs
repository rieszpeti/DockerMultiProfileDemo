using DockerMultiProfileDemo.Database;
using Microsoft.EntityFrameworkCore;

namespace DockerMultiProfileDemo.Extensions
{
    internal static class DbExtension
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app?.ApplicationServices.CreateScope();

            using var dbContext = scope?.ServiceProvider.GetRequiredService<AppDbContext>();

            dbContext?.Database.Migrate();
        }
    }
}
