using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSepayPaymentTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAtUtc",
                table: "PlusRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SepayTransactionId",
                table: "PlusRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlusRequests_SepayTransactionId",
                table: "PlusRequests",
                column: "SepayTransactionId",
                unique: true,
                filter: "[SepayTransactionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlusRequests_SepayTransactionId",
                table: "PlusRequests");

            migrationBuilder.DropColumn(
                name: "PaidAtUtc",
                table: "PlusRequests");

            migrationBuilder.DropColumn(
                name: "SepayTransactionId",
                table: "PlusRequests");
        }
    }
}
