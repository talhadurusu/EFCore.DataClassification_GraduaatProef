using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class adminhasusers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AdminId",
                table: "Users",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AdminId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Users");
        }
    }
}
