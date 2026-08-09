using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                table: "Documents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UserId_ContentHash",
                table: "Documents",
                columns: new[] { "UserId", "ContentHash" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_UserId_ContentHash",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "Documents");
        }
    }
}
