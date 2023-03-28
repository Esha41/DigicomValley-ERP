using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ERP_Project.Migrations
{
    public partial class AddDepTeamsHeadINLeaves : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<int>(
            //    name: "Department_Teams_HeadsId",
            //    table: "Leaves",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Leaves_Department_Teams_HeadsId",
            //    table: "Leaves",
            //    column: "Department_Teams_HeadsId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Leaves_Department_Teams_Heads_Department_Teams_HeadsId",
            //    table: "Leaves",
            //    column: "Department_Teams_HeadsId",
            //    principalTable: "Department_Teams_Heads",
            //    principalColumn: "Department_Teams_HeadsId",
            //    onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Leaves_Department_Teams_Heads_Department_Teams_HeadsId",
            //    table: "Leaves");

            //migrationBuilder.DropIndex(
            //    name: "IX_Leaves_Department_Teams_HeadsId",
            //    table: "Leaves");

            //migrationBuilder.DropColumn(
            //    name: "Department_Teams_HeadsId",
            //    table: "Leaves");

            migrationBuilder.CreateTable(
                name: "LeavesApprovels",
                columns: table => new
                {
                    LeavesApprovelRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Department_Teams_HeadsId = table.Column<int>(type: "int", nullable: true),
                    LeavesId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeavesApprovels", x => x.LeavesApprovelRequestId);
                    table.ForeignKey(
                        name: "FK_LeavesApprovels_Department_Teams_Heads_Department_Teams_HeadsId",
                        column: x => x.Department_Teams_HeadsId,
                        principalTable: "Department_Teams_Heads",
                        principalColumn: "Department_Teams_HeadsId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeavesApprovels_Leaves_LeavesId",
                        column: x => x.LeavesId,
                        principalTable: "Leaves",
                        principalColumn: "LeavesId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeavesApprovels_Department_Teams_HeadsId",
                table: "LeavesApprovels",
                column: "Department_Teams_HeadsId");

            migrationBuilder.CreateIndex(
                name: "IX_LeavesApprovels_LeavesId",
                table: "LeavesApprovels",
                column: "LeavesId");
        }
    }
}
