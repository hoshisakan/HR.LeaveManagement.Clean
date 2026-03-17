using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.LeaveManagement.Persistence.Configurations.Migrations
{
    /// <inheritdoc />
    public partial class AddUsedDaysToLeaveAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsedDays",
                table: "LeaveAllocations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateCreated", "DateModified" },
                values: new object[] { new DateTime(2026, 3, 18, 3, 36, 35, 412, DateTimeKind.Local).AddTicks(5838), new DateTime(2026, 3, 18, 3, 36, 35, 412, DateTimeKind.Local).AddTicks(5857) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedDays",
                table: "LeaveAllocations");

            migrationBuilder.UpdateData(
                table: "LeaveTypes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DateCreated", "DateModified" },
                values: new object[] { new DateTime(2026, 3, 2, 15, 48, 33, 360, DateTimeKind.Local).AddTicks(5883), new DateTime(2026, 3, 2, 15, 48, 33, 360, DateTimeKind.Local).AddTicks(5915) });
        }
    }
}
