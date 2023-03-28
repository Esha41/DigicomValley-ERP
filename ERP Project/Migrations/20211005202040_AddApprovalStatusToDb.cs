using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_Project.Migrations
{
    public partial class AddApprovalStatusToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "CheckoutApprovalRequests");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "CheckoutApprovalRequests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "CheckoutApprovalRequests");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "CheckoutApprovalRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
