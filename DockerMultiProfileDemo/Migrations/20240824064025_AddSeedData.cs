using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DockerMultiProfileDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "SomeEntityModels",
                columns: new[] { "id", "someProp" },
                values: new object[,]
                {
                    { 1, "First Entity" },
                    { 2, "Second Entity" },
                    { 3, "Third Entity" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SomeEntityModels",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SomeEntityModels",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SomeEntityModels",
                keyColumn: "id",
                keyValue: 3);
        }
    }
}
