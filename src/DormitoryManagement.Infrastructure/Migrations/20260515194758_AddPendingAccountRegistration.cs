using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingAccountRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pending_account_registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    student_code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    password_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    otp_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    last_sent_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    attempt_count = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pending_account_registrations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pending_account_registrations_Email",
                table: "pending_account_registrations",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pending_account_registrations_student_code",
                table: "pending_account_registrations",
                column: "student_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pending_account_registrations_Username",
                table: "pending_account_registrations",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pending_account_registrations");
        }
    }
}
