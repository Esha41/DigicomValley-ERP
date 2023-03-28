using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_Project.Migrations
{
    public partial class AddingReferenceUserIdToAllTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Skill",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Shifts",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "OfficialShifts",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "LeavesCategories",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Leaves",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Interviews",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "EmployeeTimmings",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "DepartmentTeams",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Departments",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Department_Designations",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "CheckoutApprovalRequests",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "AssignShifts",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Applications",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceUserId",
                table: "Applicants",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Skill");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "OfficialShifts");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "LeavesCategories");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "EmployeeTimmings");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "DepartmentTeams");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Department_Designations");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "CheckoutApprovalRequests");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "AssignShifts");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ReferenceUserId",
                table: "Applicants");
        }
    }
}
