using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_Project.Migrations
{
    public partial class AddChkoutApprovalRequestTodb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckoutApprovalRequests",
                columns: table => new
                {
                    CheckoutApprovalRequestId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseTime = table.Column<DateTime>(nullable: false),
                    IsApproved = table.Column<bool>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    EmployeeTimeRecordId = table.Column<int>(nullable: true),
                    Department_Teams_HeadsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutApprovalRequests", x => x.CheckoutApprovalRequestId);
                    table.ForeignKey(
                        name: "FK_CheckoutApprovalRequests_Department_Teams_Heads_Department_Teams_HeadsId",
                        column: x => x.Department_Teams_HeadsId,
                        principalTable: "Department_Teams_Heads",
                        principalColumn: "Department_Teams_HeadsId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckoutApprovalRequests_EmployeeTimeRecord_EmployeeTimeRecordId",
                        column: x => x.EmployeeTimeRecordId,
                        principalTable: "EmployeeTimeRecord",
                        principalColumn: "EmployeeTimeRecordId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutApprovalRequests_Department_Teams_HeadsId",
                table: "CheckoutApprovalRequests",
                column: "Department_Teams_HeadsId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutApprovalRequests_EmployeeTimeRecordId",
                table: "CheckoutApprovalRequests",
                column: "EmployeeTimeRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckoutApprovalRequests");
        }
    }
}
