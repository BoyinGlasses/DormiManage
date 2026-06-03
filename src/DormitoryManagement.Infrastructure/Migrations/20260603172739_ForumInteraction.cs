using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForumInteraction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "forum_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForumPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_forum_comments_forum_comments_ParentCommentId",
                        column: x => x.ParentCommentId,
                        principalTable: "forum_comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_comments_forum_posts_ForumPostId",
                        column: x => x.ForumPostId,
                        principalTable: "forum_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_comments_users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forum_reactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ForumPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ForumCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReactionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_reactions", x => x.Id);
                    table.CheckConstraint("CK_forum_reactions_target", "([ForumPostId] IS NOT NULL AND [ForumCommentId] IS NULL) OR ([ForumPostId] IS NULL AND [ForumCommentId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_forum_reactions_forum_comments_ForumCommentId",
                        column: x => x.ForumCommentId,
                        principalTable: "forum_comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_reactions_forum_posts_ForumPostId",
                        column: x => x.ForumPostId,
                        principalTable: "forum_posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_reactions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_forum_comments_AuthorUserId",
                table: "forum_comments",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_comments_ForumPostId_Status_CreatedAt",
                table: "forum_comments",
                columns: new[] { "ForumPostId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_forum_comments_ParentCommentId",
                table: "forum_comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_reactions_ForumCommentId",
                table: "forum_reactions",
                column: "ForumCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_reactions_ForumPostId",
                table: "forum_reactions",
                column: "ForumPostId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_reactions_UserId_ForumCommentId",
                table: "forum_reactions",
                columns: new[] { "UserId", "ForumCommentId" },
                unique: true,
                filter: "[ForumCommentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_forum_reactions_UserId_ForumPostId",
                table: "forum_reactions",
                columns: new[] { "UserId", "ForumPostId" },
                unique: true,
                filter: "[ForumPostId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "forum_reactions");

            migrationBuilder.DropTable(
                name: "forum_comments");
        }
    }
}
