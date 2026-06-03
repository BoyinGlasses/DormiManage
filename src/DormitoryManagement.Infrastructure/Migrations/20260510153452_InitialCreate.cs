using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "buildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_buildings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "fee_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_fee_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "forum_topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forum_topics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "floors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FloorNumber = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_floors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_floors_buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fee_rates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_rates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fee_rates_fee_types_FeeTypeId",
                        column: x => x.FeeTypeId,
                        principalTable: "fee_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    failed_login_count = table.Column<int>(type: "int", nullable: false),
                    locked_until = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    room_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    CurrentOccupancy = table.Column<int>(type: "int", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GenderType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FloorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rooms_buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rooms_floors_FloorId",
                        column: x => x.FloorId,
                        principalTable: "floors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Workstation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "forum_threads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TopicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                name: "managers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsBuildingManager = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_managers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_managers_buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_managers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_notifications_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_notifications_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    student_code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentRoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_students_rooms_CurrentRoomId",
                        column: x => x.CurrentRoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_students_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "utility_readings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingPeriod = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ElectricityPrevious = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ElectricityCurrent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaterPrevious = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WaterCurrent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utility_readings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_utility_readings_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forum_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                name: "contracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MonthlyFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DepositAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contracts_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contracts_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    invoice_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    billing_period = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoices_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    payment_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    transaction_ref = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "room_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_assignments_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_room_assignments_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "room_registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_registrations_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_room_registrations_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_room_registrations_users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "support_tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedToManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_support_tickets_managers_AssignedToManagerId",
                        column: x => x.AssignedToManagerId,
                        principalTable: "managers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_support_tickets_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_support_tickets_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    license_plate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vehicle_registrations_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "forum_likes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CommentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "invoice_adjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_adjustments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoice_adjustments_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_invoice_adjustments_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "invoice_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoice_items_fee_types_FeeTypeId",
                        column: x => x.FeeTypeId,
                        principalTable: "fee_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_invoice_items_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_allocations_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_allocations_payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "support_ticket_responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupportTicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_ticket_responses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_support_ticket_responses_support_tickets_SupportTicketId",
                        column: x => x.SupportTicketId,
                        principalTable: "support_tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_support_ticket_responses_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "buildings",
                columns: new[] { "Id", "Address", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), "North campus", "A", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "Building A", null, null },
                    { new Guid("40000000-0000-0000-0000-000000000002"), "South campus", "B", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, true, false, "Building B", null, null }
                });

            migrationBuilder.InsertData(
                table: "fee_types",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsDeleted", "IsRecurring", "Name", "Unit", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), "ROOM_FEE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, true, "Room fee", "month", null, null },
                    { new Guid("60000000-0000-0000-0000-000000000002"), "ELECTRICITY", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, true, "Electricity", "kWh", null, null },
                    { new Guid("60000000-0000-0000-0000-000000000003"), "WATER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, true, "Water", "m3", null, null },
                    { new Guid("60000000-0000-0000-0000-000000000004"), "INTERNET", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, true, "Internet", "month", null, null },
                    { new Guid("60000000-0000-0000-0000-000000000005"), "PARKING", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, true, "Parking", "month", null, null },
                    { new Guid("60000000-0000-0000-0000-000000000006"), "DEPOSIT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, false, "Deposit", "contract", null, null },
                    { new Guid("60000000-0000-0000-0000-000000000007"), "PENALTY", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, false, false, "Penalty", "case", null, null }
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
                table: "roles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsSystemRole", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "System administrator", true, "Admin", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Dormitory manager", true, "Manager", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Building manager", true, "BuildingManager", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Operational staff", true, "Staff", null, null },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Student resident", true, "Student", null, null }
                });

            migrationBuilder.InsertData(
                table: "fee_rates",
                columns: new[] { "Id", "Amount", "CreatedAt", "CreatedBy", "EffectiveFrom", "EffectiveTo", "FeeTypeId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("61000000-0000-0000-0000-000000000001"), 750000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("60000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("61000000-0000-0000-0000-000000000002"), 3500m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("60000000-0000-0000-0000-000000000002"), null, null },
                    { new Guid("61000000-0000-0000-0000-000000000003"), 12000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("60000000-0000-0000-0000-000000000003"), null, null },
                    { new Guid("61000000-0000-0000-0000-000000000004"), 50000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("60000000-0000-0000-0000-000000000004"), null, null },
                    { new Guid("61000000-0000-0000-0000-000000000005"), 100000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("60000000-0000-0000-0000-000000000005"), null, null },
                    { new Guid("61000000-0000-0000-0000-000000000006"), 1500000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("60000000-0000-0000-0000-000000000006"), null, null },
                    { new Guid("61000000-0000-0000-0000-000000000007"), 50000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("60000000-0000-0000-0000-000000000007"), null, null }
                });

            migrationBuilder.InsertData(
                table: "floors",
                columns: new[] { "Id", "BuildingId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "FloorNumber", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("41000000-0000-0000-0000-000000000001"), new Guid("40000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 1, false, "A - Floor 1", null, null },
                    { new Guid("41000000-0000-0000-0000-000000000002"), new Guid("40000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 2, false, "A - Floor 2", null, null },
                    { new Guid("41000000-0000-0000-0000-000000000003"), new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 1, false, "B - Floor 1", null, null },
                    { new Guid("41000000-0000-0000-0000-000000000004"), new Guid("40000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, 2, false, "B - Floor 2", null, null }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "email", "failed_login_count", "FullName", "IsDeleted", "last_login_at", "locked_until", "password_hash", "RoleId", "status", "UpdatedAt", "UpdatedBy", "username" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "admin@dorm.local", 0, "System Admin", false, null, null, "PBKDF2-SHA256$100000$1m5NmtpQuR1cWUt1IuZsAA==$+igdw75kLzMAdr9ZwOHZdVUOwdxHcC6qqFod5s0b/IY=", new Guid("10000000-0000-0000-0000-000000000001"), "Active", null, null, "admin" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "manager@dorm.local", 0, "Dormitory Manager", false, null, null, "PBKDF2-SHA256$100000$6I0jFFJIv+4NUsGSL8C4Ew==$slnpFAGN4SP0Y/mm/OIjOzlZGn3GMkbSDScB9Jl3Zr0=", new Guid("10000000-0000-0000-0000-000000000002"), "Active", null, null, "manager" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "building.manager@dorm.local", 0, "Building Manager", false, null, null, "PBKDF2-SHA256$100000$e8K7JNT1aUnLBBEmBtIfkA==$jy/aKoFChqiZaS0dmmX/h6IVuwLfBRe/6TZuoEmnMNs=", new Guid("10000000-0000-0000-0000-000000000003"), "Active", null, null, "building.manager" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "staff@dorm.local", 0, "Support Staff", false, null, null, "PBKDF2-SHA256$100000$qx7/JByPmq69YSr0wxruWQ==$uTwHMm1NDHZCjX5oNyGshKb3ElCg1HeZvr8bGNkmnO8=", new Guid("10000000-0000-0000-0000-000000000004"), "Active", null, null, "staff" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "student001@dorm.local", 0, "Nguyen Van An", false, null, null, "PBKDF2-SHA256$100000$Uhz+iKswPlBI7zYv2n8RFQ==$zLXy8E4cnIFBZm9O2rD3rzEIZ7ZVVbsw9xoGOr9pvlg=", new Guid("10000000-0000-0000-0000-000000000005"), "Active", null, null, "student001" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "student002@dorm.local", 0, "Tran Thi Binh", false, null, null, "PBKDF2-SHA256$100000$i3TTGynG7CepIetq41+tkg==$8glhqPZnA1euOka3+FUlItzyprw/2Jezrmbuo16aOIU=", new Guid("10000000-0000-0000-0000-000000000005"), "Active", null, null, "student002" },
                    { new Guid("20000000-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "student003@dorm.local", 0, "Le Minh Chau", false, null, null, "PBKDF2-SHA256$100000$XKvorDIdsAc/wevmSFOetQ==$3rL+30rLRtRmzCGBDUT2UrpmZjchD6mSOsGqH7zZg0A=", new Guid("10000000-0000-0000-0000-000000000005"), "Active", null, null, "student003" }
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
                table: "managers",
                columns: new[] { "Id", "BuildingId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "FullName", "IsBuildingManager", "IsDeleted", "PhoneNumber", "StaffCode", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Dormitory Manager", false, false, null, "MGR001", null, null, new Guid("20000000-0000-0000-0000-000000000002") },
                    { new Guid("30000000-0000-0000-0000-000000000002"), new Guid("40000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Building Manager", true, false, null, "BM001", null, null, new Guid("20000000-0000-0000-0000-000000000003") },
                    { new Guid("30000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Support Staff", false, false, null, "STF001", null, null, new Guid("20000000-0000-0000-0000-000000000004") }
                });

            migrationBuilder.InsertData(
                table: "rooms",
                columns: new[] { "Id", "BuildingId", "Capacity", "CreatedAt", "CreatedBy", "CurrentOccupancy", "DeletedAt", "DeletedBy", "FloorId", "GenderType", "IsDeleted", "MonthlyPrice", "room_number", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("42000000-0000-0000-0000-000000000001"), new Guid("40000000-0000-0000-0000-000000000001"), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, null, null, new Guid("41000000-0000-0000-0000-000000000001"), "Male", false, 750000m, "101", "Available", null, null },
                    { new Guid("42000000-0000-0000-0000-000000000002"), new Guid("40000000-0000-0000-0000-000000000001"), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, null, null, new Guid("41000000-0000-0000-0000-000000000001"), "Female", false, 750000m, "102", "Available", null, null },
                    { new Guid("42000000-0000-0000-0000-000000000003"), new Guid("40000000-0000-0000-0000-000000000001"), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, null, null, new Guid("41000000-0000-0000-0000-000000000001"), "Mixed", false, 750000m, "103", "Available", null, null },
                    { new Guid("42000000-0000-0000-0000-000000000004"), new Guid("40000000-0000-0000-0000-000000000001"), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, null, null, new Guid("41000000-0000-0000-0000-000000000002"), "Male", false, 650000m, "201", "Available", null, null },
                    { new Guid("42000000-0000-0000-0000-000000000005"), new Guid("40000000-0000-0000-0000-000000000001"), 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, null, null, new Guid("41000000-0000-0000-0000-000000000002"), "Female", false, 650000m, "202", "Available", null, null },
                    { new Guid("42000000-0000-0000-0000-000000000006"), new Guid("40000000-0000-0000-0000-000000000002"), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, null, null, new Guid("41000000-0000-0000-0000-000000000003"), "Mixed", false, 700000m, "101", "Available", null, null },
                    { new Guid("42000000-0000-0000-0000-000000000007"), new Guid("40000000-0000-0000-0000-000000000002"), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, null, null, new Guid("41000000-0000-0000-0000-000000000003"), "Male", false, 700000m, "102", "Available", null, null },
                    { new Guid("42000000-0000-0000-0000-000000000008"), new Guid("40000000-0000-0000-0000-000000000002"), 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, null, null, new Guid("41000000-0000-0000-0000-000000000003"), "Female", false, 700000m, "103", "Available", null, null },
                    { new Guid("42000000-0000-0000-0000-000000000009"), new Guid("40000000-0000-0000-0000-000000000002"), 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, null, null, new Guid("41000000-0000-0000-0000-000000000004"), "Mixed", false, 600000m, "201", "Available", null, null },
                    { new Guid("42000000-0000-0000-0000-000000000010"), new Guid("40000000-0000-0000-0000-000000000002"), 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, null, null, new Guid("41000000-0000-0000-0000-000000000004"), "Mixed", false, 600000m, "202", "Available", null, null }
                });

            migrationBuilder.InsertData(
                table: "students",
                columns: new[] { "Id", "ClassName", "CreatedAt", "CreatedBy", "CurrentRoomId", "DateOfBirth", "DeletedAt", "DeletedBy", "Department", "Email", "EnrollmentDate", "FullName", "Gender", "IsDeleted", "PhoneNumber", "Status", "student_code", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), "IT01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "Information Technology", "student001@dorm.local", null, "Nguyen Van An", "Male", false, "0900000001", "Staying", "SV001", null, null, new Guid("20000000-0000-0000-0000-000000000005") },
                    { new Guid("50000000-0000-0000-0000-000000000002"), "BA01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "Business Administration", "student002@dorm.local", null, "Tran Thi Binh", "Female", false, "0900000002", "Staying", "SV002", null, null, new Guid("20000000-0000-0000-0000-000000000006") },
                    { new Guid("50000000-0000-0000-0000-000000000003"), "AC01", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, null, "Accounting", "student003@dorm.local", null, "Le Minh Chau", "Other", false, "0900000003", "Pending", "SV003", null, null, new Guid("20000000-0000-0000-0000-000000000007") }
                });

            migrationBuilder.InsertData(
                table: "forum_comments",
                columns: new[] { "Id", "Content", "CreatedAt", "CreatedBy", "ParentCommentId", "Status", "ThreadId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { new Guid("a2000000-0000-0000-0000-000000000001"), "Early morning is usually less crowded.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Visible", new Guid("a1000000-0000-0000-0000-000000000002"), null, null, new Guid("20000000-0000-0000-0000-000000000006") });

            migrationBuilder.InsertData(
                table: "forum_likes",
                columns: new[] { "Id", "CommentId", "CreatedAt", "ThreadId", "UserId" },
                values: new object[] { new Guid("a3000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("a1000000-0000-0000-0000-000000000002"), new Guid("20000000-0000-0000-0000-000000000007") });

            migrationBuilder.InsertData(
                table: "invoices",
                columns: new[] { "Id", "billing_period", "CreatedAt", "CreatedBy", "DueDate", "invoice_code", "IssueDate", "PaidAmount", "RoomId", "Status", "StudentId", "TotalAmount", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("70000000-0000-0000-0000-000000000001"), "2026-05", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), "INV-2026-05-001", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 950000m, new Guid("42000000-0000-0000-0000-000000000001"), "Paid", new Guid("50000000-0000-0000-0000-000000000001"), 950000m, null, null },
                    { new Guid("70000000-0000-0000-0000-000000000002"), "2026-05", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), "INV-2026-05-002", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 400000m, new Guid("42000000-0000-0000-0000-000000000002"), "Partial", new Guid("50000000-0000-0000-0000-000000000002"), 900000m, null, null },
                    { new Guid("70000000-0000-0000-0000-000000000003"), "2026-05", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), "INV-2026-05-003", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0m, new Guid("42000000-0000-0000-0000-000000000006"), "Unpaid", new Guid("50000000-0000-0000-0000-000000000003"), 850000m, null, null }
                });

            migrationBuilder.InsertData(
                table: "payments",
                columns: new[] { "Id", "Amount", "CreatedAt", "CreatedBy", "Method", "PaidAt", "payment_code", "Status", "StudentId", "transaction_ref", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("80000000-0000-0000-0000-000000000001"), 950000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "MockGateway", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "PAY-2026-05-001", "Success", new Guid("50000000-0000-0000-0000-000000000001"), "MOCK-TXN-202605-001", null, null },
                    { new Guid("80000000-0000-0000-0000-000000000002"), 400000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "BankTransfer", new DateTime(2026, 5, 7, 0, 0, 0, 0, DateTimeKind.Utc), "PAY-2026-05-002", "Success", new Guid("50000000-0000-0000-0000-000000000002"), "BANK-TXN-202605-002", null, null }
                });

            migrationBuilder.InsertData(
                table: "support_tickets",
                columns: new[] { "Id", "AssignedToManagerId", "Category", "CreatedAt", "CreatedBy", "CreatedByUserId", "Description", "Priority", "ResolvedAt", "Status", "StudentId", "Title", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("90000000-0000-0000-0000-000000000001"), new Guid("30000000-0000-0000-0000-000000000003"), "Maintenance", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("20000000-0000-0000-0000-000000000005"), "Room A101 needs a light bulb replacement.", "Medium", null, "InProgress", new Guid("50000000-0000-0000-0000-000000000001"), "Light bulb replacement", null, null },
                    { new Guid("90000000-0000-0000-0000-000000000002"), null, "Billing", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("20000000-0000-0000-0000-000000000006"), "Please review the water charge for May.", "Low", null, "New", new Guid("50000000-0000-0000-0000-000000000002"), "Invoice clarification", null, null }
                });

            migrationBuilder.InsertData(
                table: "forum_likes",
                columns: new[] { "Id", "CommentId", "CreatedAt", "ThreadId", "UserId" },
                values: new object[] { new Guid("a3000000-0000-0000-0000-000000000002"), new Guid("a2000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("20000000-0000-0000-0000-000000000005") });

            migrationBuilder.InsertData(
                table: "invoice_items",
                columns: new[] { "Id", "Amount", "CreatedAt", "CreatedBy", "Description", "FeeTypeId", "InvoiceId", "Quantity", "UnitPrice", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("71000000-0000-0000-0000-000000000001"), 750000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Room fee May 2026", new Guid("60000000-0000-0000-0000-000000000001"), new Guid("70000000-0000-0000-0000-000000000001"), 1m, 750000m, null, null },
                    { new Guid("71000000-0000-0000-0000-000000000002"), 140000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Electricity May 2026", new Guid("60000000-0000-0000-0000-000000000002"), new Guid("70000000-0000-0000-0000-000000000001"), 40m, 3500m, null, null },
                    { new Guid("71000000-0000-0000-0000-000000000003"), 60000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Water May 2026", new Guid("60000000-0000-0000-0000-000000000003"), new Guid("70000000-0000-0000-0000-000000000001"), 5m, 12000m, null, null },
                    { new Guid("71000000-0000-0000-0000-000000000004"), 750000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Room fee May 2026", new Guid("60000000-0000-0000-0000-000000000001"), new Guid("70000000-0000-0000-0000-000000000002"), 1m, 750000m, null, null },
                    { new Guid("71000000-0000-0000-0000-000000000005"), 50000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Internet May 2026", new Guid("60000000-0000-0000-0000-000000000004"), new Guid("70000000-0000-0000-0000-000000000002"), 1m, 50000m, null, null },
                    { new Guid("71000000-0000-0000-0000-000000000006"), 100000.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Water May 2026", new Guid("60000000-0000-0000-0000-000000000003"), new Guid("70000000-0000-0000-0000-000000000002"), 8.333333m, 12000m, null, null },
                    { new Guid("71000000-0000-0000-0000-000000000007"), 700000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Room fee May 2026", new Guid("60000000-0000-0000-0000-000000000001"), new Guid("70000000-0000-0000-0000-000000000003"), 1m, 700000m, null, null },
                    { new Guid("71000000-0000-0000-0000-000000000008"), 50000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Internet May 2026", new Guid("60000000-0000-0000-0000-000000000004"), new Guid("70000000-0000-0000-0000-000000000003"), 1m, 50000m, null, null },
                    { new Guid("71000000-0000-0000-0000-000000000009"), 100000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Parking May 2026", new Guid("60000000-0000-0000-0000-000000000005"), new Guid("70000000-0000-0000-0000-000000000003"), 1m, 100000m, null, null }
                });

            migrationBuilder.InsertData(
                table: "payment_allocations",
                columns: new[] { "Id", "Amount", "CreatedAt", "CreatedBy", "InvoiceId", "PaymentId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("81000000-0000-0000-0000-000000000001"), 950000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("70000000-0000-0000-0000-000000000001"), new Guid("80000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("81000000-0000-0000-0000-000000000002"), 400000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("70000000-0000-0000-0000-000000000002"), new Guid("80000000-0000-0000-0000-000000000002"), null, null }
                });

            migrationBuilder.InsertData(
                table: "support_ticket_responses",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Message", "RespondedAt", "SupportTicketId", "UpdatedAt", "UpdatedBy", "UserId" },
                values: new object[] { new Guid("91000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Maintenance staff has been assigned.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("90000000-0000-0000-0000-000000000001"), null, null, new Guid("20000000-0000-0000-0000-000000000004") });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_buildings_Code",
                table: "buildings",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contracts_ContractNumber",
                table: "contracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contracts_RoomId",
                table: "contracts",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_StudentId",
                table: "contracts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_fee_rates_FeeTypeId",
                table: "fee_rates",
                column: "FeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_fee_types_Code",
                table: "fee_types",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_floors_BuildingId_FloorNumber",
                table: "floors",
                columns: new[] { "BuildingId", "FloorNumber" },
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

            migrationBuilder.CreateIndex(
                name: "IX_invoice_adjustments_CreatedByUserId",
                table: "invoice_adjustments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_adjustments_InvoiceId",
                table: "invoice_adjustments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_FeeTypeId",
                table: "invoice_items",
                column: "FeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_InvoiceId",
                table: "invoice_items",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_invoice_code",
                table: "invoices",
                column: "invoice_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_RoomId",
                table: "invoices",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_StudentId_RoomId_billing_period",
                table: "invoices",
                columns: new[] { "StudentId", "RoomId", "billing_period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_managers_BuildingId",
                table: "managers",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_managers_StaffCode",
                table: "managers",
                column: "StaffCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_managers_UserId",
                table: "managers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_InvoiceId",
                table: "payment_allocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_PaymentId",
                table: "payment_allocations",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_payment_code",
                table: "payments",
                column: "payment_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_StudentId",
                table: "payments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_transaction_ref",
                table: "payments",
                column: "transaction_ref",
                unique: true,
                filter: "[transaction_ref] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Name",
                table: "roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_room_assignments_RoomId",
                table: "room_assignments",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_room_assignments_StudentId",
                table: "room_assignments",
                column: "StudentId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_room_registrations_ReviewedByUserId",
                table: "room_registrations",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_room_registrations_RoomId",
                table: "room_registrations",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_room_registrations_StudentId",
                table: "room_registrations",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_BuildingId_FloorId_room_number",
                table: "rooms",
                columns: new[] { "BuildingId", "FloorId", "room_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_FloorId",
                table: "rooms",
                column: "FloorId");

            migrationBuilder.CreateIndex(
                name: "IX_students_CurrentRoomId",
                table: "students",
                column: "CurrentRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_students_student_code",
                table: "students",
                column: "student_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_UserId",
                table: "students",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_support_ticket_responses_SupportTicketId",
                table: "support_ticket_responses",
                column: "SupportTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_support_ticket_responses_UserId",
                table: "support_ticket_responses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_AssignedToManagerId",
                table: "support_tickets",
                column: "AssignedToManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_CreatedByUserId",
                table: "support_tickets",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_StudentId",
                table: "support_tickets",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_user_notifications_NotificationId_UserId",
                table: "user_notifications",
                columns: new[] { "NotificationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_notifications_UserId",
                table: "user_notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleId",
                table: "users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utility_readings_RoomId_BillingPeriod",
                table: "utility_readings",
                columns: new[] { "RoomId", "BillingPeriod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_registrations_license_plate",
                table: "vehicle_registrations",
                column: "license_plate",
                unique: true,
                filter: "[status] IN ('Pending','Approved')");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_registrations_StudentId",
                table: "vehicle_registrations",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "contracts");

            migrationBuilder.DropTable(
                name: "fee_rates");

            migrationBuilder.DropTable(
                name: "forum_likes");

            migrationBuilder.DropTable(
                name: "invoice_adjustments");

            migrationBuilder.DropTable(
                name: "invoice_items");

            migrationBuilder.DropTable(
                name: "payment_allocations");

            migrationBuilder.DropTable(
                name: "room_assignments");

            migrationBuilder.DropTable(
                name: "room_registrations");

            migrationBuilder.DropTable(
                name: "support_ticket_responses");

            migrationBuilder.DropTable(
                name: "user_notifications");

            migrationBuilder.DropTable(
                name: "utility_readings");

            migrationBuilder.DropTable(
                name: "vehicle_registrations");

            migrationBuilder.DropTable(
                name: "forum_comments");

            migrationBuilder.DropTable(
                name: "fee_types");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "support_tickets");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "forum_threads");

            migrationBuilder.DropTable(
                name: "managers");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "forum_topics");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "floors");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "buildings");
        }
    }
}
