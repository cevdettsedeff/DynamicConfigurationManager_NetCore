using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ConfigurationReader.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ConfigurationItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ConfigurationItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ConfigurationItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ConfigurationItems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ConfigurationItems",
                keyColumn: "Id",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ConfigurationItems",
                columns: new[] { "Id", "ApplicationName", "CreatedAt", "IsActive", "Name", "Type", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 1, "SERVICE-A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "SiteName", "String", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "soty.io" },
                    { 2, "SERVICE-B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "IsBasketEnabled", "Bool", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "true" }
                });

            migrationBuilder.InsertData(
                table: "ConfigurationItems",
                columns: new[] { "Id", "ApplicationName", "CreatedAt", "Name", "Type", "UpdatedAt", "Value" },
                values: new object[] { 3, "SERVICE-A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "MaxItemCount", "Int", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "50" });

            migrationBuilder.InsertData(
                table: "ConfigurationItems",
                columns: new[] { "Id", "ApplicationName", "CreatedAt", "IsActive", "Name", "Type", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { 4, "SERVICE-B", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "MaxItemCount", "Int", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "100" },
                    { 5, "SERVICE-A", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "CacheTimeout", "Double", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "30.5" }
                });
        }
    }
}
