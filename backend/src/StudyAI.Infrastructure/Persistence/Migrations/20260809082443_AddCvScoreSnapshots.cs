using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCvScoreSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CvScoreSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExperienceLevel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    JobDescriptionHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InputTokens = table.Column<int>(type: "int", nullable: false),
                    OutputTokens = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CvScoreSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CvScoreSnapshots_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CvScoreSnapshots_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CvScoreSnapshots_DocumentId",
                table: "CvScoreSnapshots",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CvScoreSnapshots_UserId_DocumentId_TargetRole_ExperienceLevel_JobDescriptionHash",
                table: "CvScoreSnapshots",
                columns: new[] { "UserId", "DocumentId", "TargetRole", "ExperienceLevel", "JobDescriptionHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CvScoreSnapshots");
        }
    }
}
