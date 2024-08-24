using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using DockerMultiProfileDemo.Database;
using DockerMultiProfileDemo.Services;

namespace UnitTest
{
    public class DbServiceTests
    {
        private DbContextOptions<AppDbContext> CreateNewContextOptions()
        {
            // Create a unique name for each test database
            var databaseName = Guid.NewGuid().ToString();
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
        }

        [Fact]
        public async Task ReturnFirst_ShouldReturnFirstEntity()
        {
            // Arrange
            var options = CreateNewContextOptions();

            using (var context = new AppDbContext(options))
            {
                context.SomeEntityModels.Add(new SomeEntityModel(1, "Value1"));
                context.SomeEntityModels.Add(new SomeEntityModel(2, "Value2"));
                context.SomeEntityModels.Add(new SomeEntityModel(3, "Value3"));
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var dbService = new DbService(context);
                var result = await dbService.ReturnFirst();

                // Assert
                var expected = new SomeEntityModel(1, "Value1");
                Assert.Equal(expected, result);
            }
        }

        [Fact]
        public async Task ReturnFirst_ShouldReturnNullWhenNoEntities()
        {
            // Arrange
            var options = CreateNewContextOptions();

            // Ensure the database is empty
            using (var context = new AppDbContext(options))
            {
                // No data is added here
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new AppDbContext(options))
            {
                var dbService = new DbService(context);
                var result = await dbService.ReturnFirst();

                // Assert
                Assert.Null(result);
            }
        }
    }
}
