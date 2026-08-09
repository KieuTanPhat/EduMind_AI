using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentOrderExpiry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAtUtc",
                table: "PlusRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiresAtUtc",
                table: "PlusRequests");
        }
    }
}
