using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_Project.Migrations
{
    public partial class Addat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAttendanceRecord_EmployeeTimeRecord_EmployeeTimeRecordId",
                table: "EmployeeAttendanceRecord");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAttendanceRecord_EmployeeTimeRecordId",
                table: "EmployeeAttendanceRecord");

            migrationBuilder.DropColumn(
                name: "EmployeeTimeRecordId",
                table: "EmployeeAttendanceRecord");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "EmployeeAttendanceRecord");

            migrationBuilder.DropColumn(
                name: "IsPresent",
                table: "EmployeeAttendanceRecord");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "EmployeeAttendanceRecord",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "EmployeeAttendanceRecord");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeTimeRecordId",
                table: "EmployeeAttendanceRecord",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "EmployeeAttendanceRecord",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPresent",
                table: "EmployeeAttendanceRecord",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendanceRecord_EmployeeTimeRecordId",
                table: "EmployeeAttendanceRecord",
                column: "EmployeeTimeRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAttendanceRecord_EmployeeTimeRecord_EmployeeTimeRecordId",
                table: "EmployeeAttendanceRecord",
                column: "EmployeeTimeRecordId",
                principalTable: "EmployeeTimeRecord",
                principalColumn: "EmployeeTimeRecordId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
