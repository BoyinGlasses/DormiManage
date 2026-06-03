using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VehicleParkingRegistrationBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_vehicle_registrations_license_plate",
                table: "vehicle_registrations");

            migrationBuilder.AddColumn<decimal>(
                name: "amount",
                table: "vehicle_registrations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "invoice_id",
                table: "vehicle_registrations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "month_count",
                table: "vehicle_registrations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "payment_date",
                table: "vehicle_registrations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_registrations_invoice_id",
                table: "vehicle_registrations",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_registrations_license_plate",
                table: "vehicle_registrations",
                column: "license_plate");

            migrationBuilder.AddForeignKey(
                name: "FK_vehicle_registrations_invoices_invoice_id",
                table: "vehicle_registrations",
                column: "invoice_id",
                principalTable: "invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vehicle_registrations_invoices_invoice_id",
                table: "vehicle_registrations");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_registrations_invoice_id",
                table: "vehicle_registrations");

            migrationBuilder.DropColumn(
                name: "amount",
                table: "vehicle_registrations");

            migrationBuilder.DropColumn(
                name: "invoice_id",
                table: "vehicle_registrations");

            migrationBuilder.DropColumn(
                name: "month_count",
                table: "vehicle_registrations");

            migrationBuilder.DropColumn(
                name: "payment_date",
                table: "vehicle_registrations");

            migrationBuilder.DropIndex(
                name: "IX_vehicle_registrations_license_plate",
                table: "vehicle_registrations");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_registrations_license_plate",
                table: "vehicle_registrations",
                column: "license_plate",
                unique: true,
                filter: "[status] IN ('Pending','Approved')");
        }
    }
}
