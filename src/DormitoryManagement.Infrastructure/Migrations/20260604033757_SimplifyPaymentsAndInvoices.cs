using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyPaymentsAndInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_invoices_TargetInvoiceId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_TargetInvoiceId",
                table: "payments");

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                table: "payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VisibilityBuildingId",
                table: "forum_posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisibilityRoleName",
                table: "forum_posts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VisibilityRoomId",
                table: "forum_posts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisibilityScope",
                table: "forum_posts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT PaymentId
                    FROM payment_allocations
                    GROUP BY PaymentId
                    HAVING COUNT(*) > 1
                )
                THROW 51000, 'Cannot migrate payments with multiple allocations.', 1;
                """);

            migrationBuilder.Sql("""
                UPDATE p
                SET InvoiceId = COALESCE(p.TargetInvoiceId, pa.InvoiceId)
                FROM payments p
                LEFT JOIN payment_allocations pa ON pa.PaymentId = p.Id;
                """);

            migrationBuilder.Sql("""
                UPDATE invoices
                SET
                    Status = CASE
                        WHEN PaidAmount >= TotalAmount THEN 'Paid'
                        WHEN DueDate < GETDATE() THEN 'Overdue'
                        ELSE 'Unpaid'
                    END,
                    PaidAmount = CASE
                        WHEN PaidAmount >= TotalAmount THEN TotalAmount
                        ELSE 0
                    END
                WHERE Status = 'Partial';
                """);

            migrationBuilder.Sql("""
                UPDATE payments
                SET Method = 'QrBanking'
                WHERE Method NOT IN ('Cash', 'QrBanking');
                """);

            migrationBuilder.Sql("""
                DELETE FROM payments
                WHERE InvoiceId IS NULL AND Status <> 'Success';
                """);

            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM payments
                    WHERE InvoiceId IS NULL AND Status = 'Success'
                )
                THROW 51001, 'Cannot migrate successful payments without an invoice.', 1;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "InvoiceId",
                table: "payments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "TargetInvoiceId",
                table: "payments");

            migrationBuilder.DropTable(
                name: "payment_allocations");

            migrationBuilder.UpdateData(
                table: "invoices",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"),
                columns: new[] { "PaidAmount", "Status" },
                values: new object[] { 900000m, "Paid" });

            migrationBuilder.UpdateData(
                table: "payments",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"),
                columns: new[] { "InvoiceId", "Method", "TransactionRef" },
                values: new object[] { new Guid("70000000-0000-0000-0000-000000000001"), "QrBanking", "QR-TXN-202605-001" });

            migrationBuilder.UpdateData(
                table: "payments",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000002"),
                columns: new[] { "Amount", "InvoiceId", "Method" },
                values: new object[] { 900000m, new Guid("70000000-0000-0000-0000-000000000002"), "QrBanking" });

            migrationBuilder.CreateIndex(
                name: "IX_payments_InvoiceId",
                table: "payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_forum_posts_VisibilityScope_VisibilityBuildingId_VisibilityRoomId",
                table: "forum_posts",
                columns: new[] { "VisibilityScope", "VisibilityBuildingId", "VisibilityRoomId" });

            migrationBuilder.AddForeignKey(
                name: "FK_payments_invoices_InvoiceId",
                table: "payments",
                column: "InvoiceId",
                principalTable: "invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_invoices_InvoiceId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_payments_InvoiceId",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "IX_forum_posts_VisibilityScope_VisibilityBuildingId_VisibilityRoomId",
                table: "forum_posts");

            migrationBuilder.DropColumn(
                name: "VisibilityBuildingId",
                table: "forum_posts");

            migrationBuilder.DropColumn(
                name: "VisibilityRoleName",
                table: "forum_posts");

            migrationBuilder.DropColumn(
                name: "VisibilityRoomId",
                table: "forum_posts");

            migrationBuilder.DropColumn(
                name: "VisibilityScope",
                table: "forum_posts");

            migrationBuilder.AddColumn<Guid>(
                name: "TargetInvoiceId",
                table: "payments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE payments
                SET TargetInvoiceId = InvoiceId;
                """);

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "payments");

            migrationBuilder.CreateTable(
                name: "payment_allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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

            migrationBuilder.UpdateData(
                table: "invoices",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"),
                columns: new[] { "PaidAmount", "Status" },
                values: new object[] { 400000m, "Partial" });

            migrationBuilder.InsertData(
                table: "payment_allocations",
                columns: new[] { "Id", "Amount", "CreatedAt", "CreatedBy", "InvoiceId", "PaymentId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("81000000-0000-0000-0000-000000000001"), 950000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("70000000-0000-0000-0000-000000000001"), new Guid("80000000-0000-0000-0000-000000000001"), null, null },
                    { new Guid("81000000-0000-0000-0000-000000000002"), 400000m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("70000000-0000-0000-0000-000000000002"), new Guid("80000000-0000-0000-0000-000000000002"), null, null }
                });

            migrationBuilder.UpdateData(
                table: "payments",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"),
                columns: new[] { "Method", "TargetInvoiceId" },
                values: new object[] { "MockGateway", null });

            migrationBuilder.UpdateData(
                table: "payments",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000002"),
                columns: new[] { "Amount", "Method", "TargetInvoiceId" },
                values: new object[] { 400000m, "BankTransfer", null });

            migrationBuilder.CreateIndex(
                name: "IX_payments_TargetInvoiceId",
                table: "payments",
                column: "TargetInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_InvoiceId",
                table: "payment_allocations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_allocations_PaymentId",
                table: "payment_allocations",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_invoices_TargetInvoiceId",
                table: "payments",
                column: "TargetInvoiceId",
                principalTable: "invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
