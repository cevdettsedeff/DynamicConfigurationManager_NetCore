using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ConfigurationReader.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigurationItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ApplicationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationItems", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationName_IsActive",
                table: "ConfigurationItems",
                columns: new[] { "ApplicationName", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationName_Name",
                table: "ConfigurationItems",
                columns: new[] { "ApplicationName", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UpdatedAt",
                table: "ConfigurationItems",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationItems");
        }
    }
}
