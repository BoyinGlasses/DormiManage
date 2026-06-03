using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QrBankingAutoPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bank_transaction_id",
                table: "invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "qr_data_url",
                table: "invoices",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transfer_content",
                table: "invoices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "invoices",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"),
                columns: new[] { "bank_transaction_id", "qr_data_url", "transfer_content" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "invoices",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"),
                columns: new[] { "bank_transaction_id", "qr_data_url", "transfer_content" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "invoices",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"),
                columns: new[] { "bank_transaction_id", "qr_data_url", "transfer_content" },
                values: new object[] { null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_invoices_bank_transaction_id",
                table: "invoices",
                column: "bank_transaction_id",
                unique: true,
                filter: "[bank_transaction_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_transfer_content",
                table: "invoices",
                column: "transfer_content",
                unique: true,
                filter: "[transfer_content] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_invoices_bank_transaction_id",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_transfer_content",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "bank_transaction_id",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "qr_data_url",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "transfer_content",
                table: "invoices");
        }
    }
}
