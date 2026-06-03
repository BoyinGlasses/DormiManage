using System;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    [DbContext(typeof(DormitoryDbContext))]
    [Migration("20260603000100_RestoreForumCore")]
    public partial class RestoreForumCore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forum_categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "forum_tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "forum_posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisibilityScope = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetRoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_posts", x => x.Id);
                    table.ForeignKey("FK_forum_posts_buildings_BuildingId", x => x.BuildingId, "buildings", "Id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_forum_posts_forum_categories_CategoryId", x => x.CategoryId, "forum_categories", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_forum_posts_rooms_RoomId", x => x.RoomId, "rooms", "Id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey("FK_forum_posts_users_CreatedByUserId", x => x.CreatedByUserId, "users", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forum_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_comments", x => x.Id);
                    table.ForeignKey("FK_forum_comments_forum_comments_ParentCommentId", x => x.ParentCommentId, "forum_comments", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_forum_comments_forum_posts_PostId", x => x.PostId, "forum_posts", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_forum_comments_users_CreatedByUserId", x => x.CreatedByUserId, "users", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forum_post_tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_post_tags", x => x.Id);
                    table.ForeignKey("FK_forum_post_tags_forum_posts_PostId", x => x.PostId, "forum_posts", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_forum_post_tags_forum_tags_TagId", x => x.TagId, "forum_tags", "Id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex("IX_forum_categories_Code", "forum_categories", "Code", unique: true);
            migrationBuilder.CreateIndex("IX_forum_tags_Slug", "forum_tags", "Slug", unique: true);
            migrationBuilder.CreateIndex("IX_forum_posts_BuildingId", "forum_posts", "BuildingId");
            migrationBuilder.CreateIndex("IX_forum_posts_CategoryId", "forum_posts", "CategoryId");
            migrationBuilder.CreateIndex("IX_forum_posts_CreatedByUserId", "forum_posts", "CreatedByUserId");
            migrationBuilder.CreateIndex("IX_forum_posts_RoomId", "forum_posts", "RoomId");
            migrationBuilder.CreateIndex("IX_forum_posts_VisibilityScope_BuildingId_RoomId_TargetRoleName", "forum_posts", new[] { "VisibilityScope", "BuildingId", "RoomId", "TargetRoleName" });
            migrationBuilder.CreateIndex("IX_forum_comments_CreatedByUserId", "forum_comments", "CreatedByUserId");
            migrationBuilder.CreateIndex("IX_forum_comments_ParentCommentId", "forum_comments", "ParentCommentId");
            migrationBuilder.CreateIndex("IX_forum_comments_PostId", "forum_comments", "PostId");
            migrationBuilder.CreateIndex("IX_forum_post_tags_PostId_TagId", "forum_post_tags", new[] { "PostId", "TagId" }, unique: true);
            migrationBuilder.CreateIndex("IX_forum_post_tags_TagId", "forum_post_tags", "TagId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "forum_comments");
            migrationBuilder.DropTable(name: "forum_post_tags");
            migrationBuilder.DropTable(name: "forum_posts");
            migrationBuilder.DropTable(name: "forum_tags");
            migrationBuilder.DropTable(name: "forum_categories");
        }
    }
}
