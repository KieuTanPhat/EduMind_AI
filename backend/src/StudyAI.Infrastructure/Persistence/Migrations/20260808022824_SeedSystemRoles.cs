using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudyAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSystemRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAtUtc", "Name", "NormalizedName", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("a1d8f3c5-7c0f-4e2d-a1c2-3fd9e7d18c01"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "User", "USER", null },
                    { new Guid("b2e9f4d6-8d10-4f3e-b2d3-4fe0f8e29d12"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admin", "ADMIN", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1d8f3c5-7c0f-4e2d-a1c2-3fd9e7d18c01"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b2e9f4d6-8d10-4f3e-b2d3-4fe0f8e29d12"));
        }
    }
}
