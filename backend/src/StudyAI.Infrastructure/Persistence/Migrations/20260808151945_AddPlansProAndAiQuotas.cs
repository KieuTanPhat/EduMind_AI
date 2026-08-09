using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPlansProAndAiQuotas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AiTokenLimitPerDay",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Plan",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Free");

            migrationBuilder.AddColumn<string>(
                name: "Plan",
                table: "PlusRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Plus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiTokenLimitPerDay",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Plan",
                table: "PlusRequests");
        }
    }
}
