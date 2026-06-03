using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_audit_logs_users_UserId",
                table: "audit_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_user_notifications_notifications_NotificationId",
                table: "user_notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_user_notifications_users_UserId",
                table: "user_notifications");

            migrationBuilder.DropTable(
                name: "forum_likes");

            migrationBuilder.DropTable(
                name: "forum_comments");

            migrationBuilder.DropTable(
                name: "forum_threads");

            migrationBuilder.DropTable(
                name: "forum_topics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notifications",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_notifications",
                table: "user_notifications");

            migrationBuilder.DropIndex(
                name: "IX_user_notifications_NotificationId_UserId",
                table: "user_notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs");

            migrationBuilder.RenameTable(
                name: "notifications",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "user_notifications",
                newName: "UserNotifications");

            migrationBuilder.RenameTable(
                name: "audit_logs",
                newName: "AuditLogs");

            migrationBuilder.RenameIndex(
                name: "IX_user_notifications_UserId",
                table: "UserNotifications",
                newName: "IX_UserNotifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_audit_logs_UserId",
                table: "AuditLogs",
                newName: "IX_AuditLogs_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "AuditLogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserNotifications",
                table: "UserNotifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_NotificationId",
                table: "UserNotifications",
                column: "NotificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_users_UserId",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_Notifications_NotificationId",
                table: "UserNotifications",
                column: "NotificationId",
                principalTable: "Notifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserNotifications_users_UserId",
                table: "UserNotifications",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_users_UserId",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_Notifications_NotificationId",
                table: "UserNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_UserNotifications_users_UserId",
                table: "UserNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserNotifications",
                table: "UserNotifications");

            migrationBuilder.DropIndex(
                name: "IX_UserNotifications_NotificationId",
                table: "UserNotifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditLogs",
                table: "AuditLogs");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "notifications");

            migrationBuilder.RenameTable(
                name: "UserNotifications",
                newName: "user_notifications");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                newName: "audit_logs");

            migrationBuilder.RenameIndex(
                name: "IX_UserNotifications_UserId",
                table: "user_notifications",
                newName: "IX_user_notifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_AuditLogs_UserId",
                table: "audit_logs",
                newName: "IX_audit_logs_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "notifications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EntityName",
                table: "audit_logs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                table: "audit_logs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notifications",
                table: "notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_notifications",
                table: "user_notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_audit_logs",
                table: "audit_logs",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "forum_topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_topics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "forum_threads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_threads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_forum_threads_forum_topics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "forum_topics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_threads_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forum_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentCommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
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
                        name: "FK_forum_comments_forum_threads_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "forum_threads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_forum_comments_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forum_likes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_forum_likes_forum_comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "forum_comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_likes_forum_threads_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "forum_threads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_forum_likes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "forum_topics",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "Status", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Official dormitory announcements.", "Visible", "Announcements", null, null },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Student discussions and questions.", "Visible", "Resident Community", null, null }
                });

            migrationBuilder.InsertData(
                table: "forum_threads",
                columns: new[] { "Id", "Content", "CreatedAt", "CreatedBy", "Status", "Title", "TopicId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), "This forum is for dormitory announcements and resident support.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Visible", "Welcome to DormitoryManagement", new Guid("a0000000-0000-0000-0000-000000000001"), null, null, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), "Share available time slots and useful tips for the laundry room.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Visible", "Laundry room tips", new Guid("a0000000-0000-0000-0000-000000000002"), null, null, new Guid("20000000-0000-0000-0000-000000000005") }
                });

            migrationBuilder.InsertData(
                table: "forum_comments",
                columns: new[] { "Id", "Content", "CreatedAt", "CreatedBy", "ParentCommentId", "Status", "ThreadId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { new Guid("a2000000-0000-0000-0000-000000000001"), "Early morning is usually less crowded.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Visible", new Guid("a1000000-0000-0000-0000-000000000002"), null, null, new Guid("20000000-0000-0000-0000-000000000006") });

            migrationBuilder.InsertData(
                table: "forum_likes",
                columns: new[] { "Id", "CommentId", "CreatedAt", "ThreadId", "UserId" },
                values: new object[,]
                {
                    { new Guid("a3000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1000000-0000-0000-0000-000000000002"), new Guid("20000000-0000-0000-0000-000000000007") },
                    { new Guid("a3000000-0000-0000-0000-000000000002"), new Guid("a2000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("20000000-0000-0000-0000-000000000005") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_notifications_NotificationId_UserId",
                table: "user_notifications",
                columns: new[] { "NotificationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_forum_comments_ParentCommentId",
                table: "forum_comments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_comments_ThreadId",
                table: "forum_comments",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_comments_UserId",
                table: "forum_comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_likes_CommentId",
                table: "forum_likes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_likes_ThreadId",
                table: "forum_likes",
                column: "ThreadId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_likes_UserId_CommentId",
                table: "forum_likes",
                columns: new[] { "UserId", "CommentId" },
                unique: true,
                filter: "[CommentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_forum_likes_UserId_ThreadId",
                table: "forum_likes",
                columns: new[] { "UserId", "ThreadId" },
                unique: true,
                filter: "[ThreadId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_forum_threads_TopicId",
                table: "forum_threads",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_threads_UserId",
                table: "forum_threads",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_audit_logs_users_UserId",
                table: "audit_logs",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_user_notifications_notifications_NotificationId",
                table: "user_notifications",
                column: "NotificationId",
                principalTable: "notifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_user_notifications_users_UserId",
                table: "user_notifications",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
