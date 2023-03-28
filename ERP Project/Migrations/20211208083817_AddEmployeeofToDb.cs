using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_Project.Migrations
{
    public partial class AddEmployeeofToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployeeOf",
                table: "Employees",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeOf",
                table: "Employees");
        }
    }
}
