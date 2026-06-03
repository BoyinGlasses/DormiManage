using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BillingRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_invoices_StudentId_RoomId_billing_period",
                table: "invoices");

            migrationBuilder.AddColumn<int>(
                name: "ContractTermMonths",
                table: "room_registrations",
                type: "int",
                nullable: false,
                defaultValue: 12);

            migrationBuilder.AddColumn<bool>(
                name: "IncludesInternet",
                table: "room_registrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetInvoiceId",
                table: "payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ContractId",
                table: "invoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceKind",
                table: "invoices",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "MonthlyUtility");

            migrationBuilder.AddColumn<bool>(
                name: "IncludesInternet",
                table: "contracts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomRegistrationId",
                table: "contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TermMonths",
                table: "contracts",
                type: "int",
                nullable: false,
                defaultValue: 12);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "contracts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "UpfrontInvoiceId",
                table: "contracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "payment_extensions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedDueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_extensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_extensions_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_extensions_students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payment_extensions_users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "fee_rates",
                keyColumn: "Id",
                keyValue: new Guid("61000000-0000-0000-0000-000000000003"),
                column: "Amount",
                value: 19500m);

            migrationBuilder.UpdateData(
                table: "invoices",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"),
                columns: new[] { "ContractId", "InvoiceKind" },
                values: new object[] { null, "MonthlyUtility" });

            migrationBuilder.UpdateData(
                table: "invoices",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"),
                columns: new[] { "ContractId", "InvoiceKind" },
                values: new object[] { null, "MonthlyUtility" });

            migrationBuilder.UpdateData(
                table: "invoices",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"),
                columns: new[] { "ContractId", "InvoiceKind" },
                values: new object[] { null, "MonthlyUtility" });

            migrationBuilder.UpdateData(
                table: "payments",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"),
                column: "TargetInvoiceId",
                value: null);

            migrationBuilder.UpdateData(
                table: "payments",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000002"),
                column: "TargetInvoiceId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_payments_TargetInvoiceId",
                table: "payments",
                column: "TargetInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_ContractId",
                table: "invoices",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_StudentId_RoomId_billing_period_InvoiceKind",
                table: "invoices",
                columns: new[] { "StudentId", "RoomId", "billing_period", "InvoiceKind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_contracts_RoomRegistrationId",
                table: "contracts",
                column: "RoomRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_contracts_UpfrontInvoiceId",
                table: "contracts",
                column: "UpfrontInvoiceId",
                unique: true,
                filter: "[UpfrontInvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payment_extensions_InvoiceId_Status",
                table: "payment_extensions",
                columns: new[] { "InvoiceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_extensions_ReviewedByUserId",
                table: "payment_extensions",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_extensions_StudentId",
                table: "payment_extensions",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_contracts_invoices_UpfrontInvoiceId",
                table: "contracts",
                column: "UpfrontInvoiceId",
                principalTable: "invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_contracts_room_registrations_RoomRegistrationId",
                table: "contracts",
                column: "RoomRegistrationId",
                principalTable: "room_registrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_invoices_contracts_ContractId",
                table: "invoices",
                column: "ContractId",
                principalTable: "contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_payments_invoices_TargetInvoiceId",
                table: "payments",
                column: "TargetInvoiceId",
                principalTable: "invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_contracts_invoices_UpfrontInvoiceId",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_contracts_room_registrations_RoomRegistrationId",
                table: "contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_invoices_contracts_ContractId",
                table: "invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_payments_invoices_TargetInvoiceId",
                table: "payments");

            migrationBuilder.DropTable(
                name: "payment_extensions");

            migrationBuilder.DropIndex(
                name: "IX_payments_TargetInvoiceId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_invoices_ContractId",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_StudentId_RoomId_billing_period_InvoiceKind",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_contracts_RoomRegistrationId",
                table: "contracts");

            migrationBuilder.DropIndex(
                name: "IX_contracts_UpfrontInvoiceId",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "ContractTermMonths",
                table: "room_registrations");

            migrationBuilder.DropColumn(
                name: "IncludesInternet",
                table: "room_registrations");

            migrationBuilder.DropColumn(
                name: "TargetInvoiceId",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "ContractId",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "InvoiceKind",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "IncludesInternet",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "RoomRegistrationId",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "TermMonths",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "UpfrontInvoiceId",
                table: "contracts");

            migrationBuilder.UpdateData(
                table: "fee_rates",
                keyColumn: "Id",
                keyValue: new Guid("61000000-0000-0000-0000-000000000003"),
                column: "Amount",
                value: 12000m);

            migrationBuilder.CreateIndex(
                name: "IX_invoices_StudentId_RoomId_billing_period",
                table: "invoices",
                columns: new[] { "StudentId", "RoomId", "billing_period" },
                unique: true);
        }
    }
}
