using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForumReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forum_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReporterUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ForumPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ForumCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionAction = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ResolutionNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_forum_reports_forum_comments_ForumCommentId",
                        column: x => x.ForumCommentId,
                        principalTable: "forum_comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_reports_forum_posts_ForumPostId",
                        column: x => x.ForumPostId,
                        principalTable: "forum_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_reports_users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_reports_users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_forum_reports_ForumCommentId",
                table: "forum_reports",
                column: "ForumCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_reports_ForumPostId",
                table: "forum_reports",
                column: "ForumPostId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_reports_ReporterUserId_ForumCommentId_Status",
                table: "forum_reports",
                columns: new[] { "ReporterUserId", "ForumCommentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_forum_reports_ReporterUserId_ForumPostId_Status",
                table: "forum_reports",
                columns: new[] { "ReporterUserId", "ForumPostId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_forum_reports_ReviewedByUserId",
                table: "forum_reports",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_reports_Status_CreatedAt",
                table: "forum_reports",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "forum_reports");
        }
    }
}
