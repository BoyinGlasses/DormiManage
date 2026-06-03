using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DormitoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDemoLoginAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "students",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "Email",
                value: "student01@ktx.local");

            migrationBuilder.UpdateData(
                table: "students",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "Email",
                value: "student02@ktx.local");

            migrationBuilder.UpdateData(
                table: "students",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "Email",
                value: "student03@ktx.local");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "email", "password_hash" },
                values: new object[] { "admin@ktx.local", "PBKDF2-SHA256$100000$JCipfxfImHrCtND9jB6Jfw==$rw/DANHB2xRcV1nmDVz0ALdGNTdJAFx11ILhkdbNdE8=" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "email", "password_hash" },
                values: new object[] { "manager@ktx.local", "PBKDF2-SHA256$100000$PamHprATyGBC3ivsgWWdhQ==$rsaYYxIMFg9L6ijRV2JKZCS2ayIezOXhhzFkkOPDza0=" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "email", "password_hash" },
                values: new object[] { "building.manager@ktx.local", "PBKDF2-SHA256$100000$shzcddxynvdmqHIpBAS/wQ==$CPeXdT7up4frQzlyddcXP9sNR7rml8supu2XIy/WdeM=" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "email", "password_hash" },
                values: new object[] { "staff@ktx.local", "PBKDF2-SHA256$100000$x6lroz/EWVMoSbusCwB7ZQ==$CZwfTn5eHRl8Ph4KsM9Fm3H712ujYp647a+6LaR1NW8=" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "email", "password_hash", "username" },
                values: new object[] { "student01@ktx.local", "PBKDF2-SHA256$100000$4+/sphDQUiSqkjtRbc/jUw==$Zxdu2PLvBHNdNvG0pMRkdO60c54ePsyI0VmbXHaz5xc=", "student01" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"),
                columns: new[] { "email", "password_hash", "username" },
                values: new object[] { "student02@ktx.local", "PBKDF2-SHA256$100000$wqCHattCX2x6ywzPp3ZMJg==$MaXh00Fbg7v2ZT3FbkrSQui4HjcrSTK0wnhMh9DGoQM=", "student02" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000007"),
                columns: new[] { "email", "password_hash", "username" },
                values: new object[] { "student03@ktx.local", "PBKDF2-SHA256$100000$hpZWt6wPlOB+vFm/FBP1pw==$XPfSDfwasR1DLPdvfPAo4Wpc7aYx5x8NP6dnPG7cloc=", "student03" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "students",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "Email",
                value: "student001@dorm.local");

            migrationBuilder.UpdateData(
                table: "students",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "Email",
                value: "student002@dorm.local");

            migrationBuilder.UpdateData(
                table: "students",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "Email",
                value: "student003@dorm.local");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "email", "password_hash" },
                values: new object[] { "admin@dorm.local", "PBKDF2-SHA256$100000$1m5NmtpQuR1cWUt1IuZsAA==$+igdw75kLzMAdr9ZwOHZdVUOwdxHcC6qqFod5s0b/IY=" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                columns: new[] { "email", "password_hash" },
                values: new object[] { "manager@dorm.local", "PBKDF2-SHA256$100000$6I0jFFJIv+4NUsGSL8C4Ew==$slnpFAGN4SP0Y/mm/OIjOzlZGn3GMkbSDScB9Jl3Zr0=" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                columns: new[] { "email", "password_hash" },
                values: new object[] { "building.manager@dorm.local", "PBKDF2-SHA256$100000$e8K7JNT1aUnLBBEmBtIfkA==$jy/aKoFChqiZaS0dmmX/h6IVuwLfBRe/6TZuoEmnMNs=" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                columns: new[] { "email", "password_hash" },
                values: new object[] { "staff@dorm.local", "PBKDF2-SHA256$100000$qx7/JByPmq69YSr0wxruWQ==$uTwHMm1NDHZCjX5oNyGshKb3ElCg1HeZvr8bGNkmnO8=" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                columns: new[] { "email", "password_hash", "username" },
                values: new object[] { "student001@dorm.local", "PBKDF2-SHA256$100000$Uhz+iKswPlBI7zYv2n8RFQ==$zLXy8E4cnIFBZm9O2rD3rzEIZ7ZVVbsw9xoGOr9pvlg=", "student001" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"),
                columns: new[] { "email", "password_hash", "username" },
                values: new object[] { "student002@dorm.local", "PBKDF2-SHA256$100000$i3TTGynG7CepIetq41+tkg==$8glhqPZnA1euOka3+FUlItzyprw/2Jezrmbuo16aOIU=", "student002" });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000007"),
                columns: new[] { "email", "password_hash", "username" },
                values: new object[] { "student003@dorm.local", "PBKDF2-SHA256$100000$XKvorDIdsAc/wevmSFOetQ==$3rL+30rLRtRmzCGBDUT2UrpmZjchD6mSOsGqH7zZg0A=", "student003" });
        }
    }
}
