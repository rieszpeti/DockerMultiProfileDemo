using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DockerMultiProfileDemo.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

        public DbSet<SomeEntityModel> SomeEntityModels => Set<SomeEntityModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SomeEntityModel>().HasData(
                new SomeEntityModel(1, "First Entity"),
                new SomeEntityModel(2, "Second Entity"),
                new SomeEntityModel(3, "Third Entity")
            );
        }
    }
}
