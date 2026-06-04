using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForumVisibilityScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VisibilityScope",
                table: "forum_posts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Dormitory");

            migrationBuilder.AddColumn<Guid>(
                name: "VisibilityBuildingId",
                table: "forum_posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VisibilityRoomId",
                table: "forum_posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisibilityRoleName",
                table: "forum_posts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_forum_posts_VisibilityScope_VisibilityBuildingId_VisibilityRoomId",
                table: "forum_posts",
                columns: new[] { "VisibilityScope", "VisibilityBuildingId", "VisibilityRoomId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_forum_posts_VisibilityScope_VisibilityBuildingId_VisibilityRoomId",
                table: "forum_posts");

            migrationBuilder.DropColumn(
                name: "VisibilityScope",
                table: "forum_posts");

            migrationBuilder.DropColumn(
                name: "VisibilityBuildingId",
                table: "forum_posts");

            migrationBuilder.DropColumn(
                name: "VisibilityRoomId",
                table: "forum_posts");

            migrationBuilder.DropColumn(
                name: "VisibilityRoleName",
                table: "forum_posts");
        }
    }
}
