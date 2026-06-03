using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VehicleParkingInvoiceMultiplicity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_invoices_StudentId_RoomId_billing_period_InvoiceKind",
                table: "invoices");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_StudentId_RoomId_billing_period_InvoiceKind",
                table: "invoices",
                columns: new[] { "StudentId", "RoomId", "billing_period", "InvoiceKind" },
                unique: true,
                filter: "[InvoiceKind] = 'MonthlyUtility'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_invoices_StudentId_RoomId_billing_period_InvoiceKind",
                table: "invoices");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_StudentId_RoomId_billing_period_InvoiceKind",
                table: "invoices",
                columns: new[] { "StudentId", "RoomId", "billing_period", "InvoiceKind" },
                unique: true);
        }
    }
}
