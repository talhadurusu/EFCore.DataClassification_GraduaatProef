using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SmokeTestEdgeCases_Phase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users");

            migrationBuilder.DropDataClassification(table: "Documents", column: "Writer");

            migrationBuilder.DropColumn(
                name: "Writer",
                table: "Documents");

            migrationBuilder.DropDataClassification(table: "Users", column: "AccountStatus");

            migrationBuilder.RenameColumn(
                name: "AccountStatus",
                table: "Users",
                newName: "Status");

            migrationBuilder.DropDataClassification(table: "Admins", column: "InscriptionNumber");

            migrationBuilder.RenameColumn(
                name: "InscriptionNumber",
                table: "Admins",
                newName: "RegistrationNumber");

            migrationBuilder.AlterColumn<int>(
                name: "AdminId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerEmail",
                table: "Car",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropDataClassification(table: "Users", column: "UserName");

            migrationBuilder.DropDataClassification(table: "Users", column: "Email");

            migrationBuilder.AddDataClassification(table: "Users", column: "Email", label: "Contact", informationType: "Email Address", rank: "High");

            migrationBuilder.AddDataClassification(table: "Users", column: "Adress", label: "Location", informationType: "Home Address", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Users", column: "Status", label: "Internal", informationType: "User Status", rank: "Low");

            migrationBuilder.DropDataClassification(table: "People", column: "ContactPhone");

            migrationBuilder.DropDataClassification(table: "Documents", column: "Summary");

            migrationBuilder.AddDataClassification(table: "Bikes", column: "Brand", label: "Public", informationType: "Bike Brand", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Admins", column: "PhoneNo", label: "Contact", informationType: "Admin Phone", rank: "Medium");

            migrationBuilder.DropDataClassification(table: "Admins", column: "LastName");

            migrationBuilder.AddDataClassification(table: "Admins", column: "LastName", label: "Confidential", informationType: "Admin Last Name", rank: "Medium");

            migrationBuilder.DropDataClassification(table: "Admins", column: "FavoriteBookAuthor");

            migrationBuilder.AddDataClassification(table: "Admins", column: "RegistrationNumber", label: "Confidential", informationType: "Admin Inscription Number", rank: "Medium");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users");

            migrationBuilder.DropDataClassification(table: "Users", column: "Status");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Users",
                newName: "AccountStatus");

            migrationBuilder.DropDataClassification(table: "Admins", column: "RegistrationNumber");

            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "Admins",
                newName: "InscriptionNumber");

            migrationBuilder.AlterColumn<int>(
                name: "AdminId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Writer",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerEmail",
                table: "Car",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddDataClassification(table: "Users", column: "UserName", label: "Personal", informationType: "User Name", rank: "Low");

            migrationBuilder.DropDataClassification(table: "Users", column: "Email");

            migrationBuilder.AddDataClassification(table: "Users", column: "Email", label: "Contact", informationType: "Email Address", rank: "Medium");

            migrationBuilder.DropDataClassification(table: "Users", column: "Adress");

            migrationBuilder.AddDataClassification(table: "Users", column: "AccountStatus", label: "Internal", informationType: "User Status", rank: "Low");

            migrationBuilder.AddDataClassification(table: "People", column: "ContactPhone", label: "Contact", informationType: "Phone Number", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Documents", column: "Summary", label: "Docs", informationType: "Summary", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Documents", column: "Writer", label: "Docs", informationType: "Writer Name", rank: "High");

            migrationBuilder.DropDataClassification(table: "Bikes", column: "Brand");

            migrationBuilder.DropDataClassification(table: "Admins", column: "PhoneNo");

            migrationBuilder.DropDataClassification(table: "Admins", column: "LastName");

            migrationBuilder.AddDataClassification(table: "Admins", column: "LastName", label: "Confidential", informationType: "Admin Last Name", rank: "High");

            migrationBuilder.AddDataClassification(table: "Admins", column: "FavoriteBookAuthor", label: "Internal", informationType: "Favorite Author", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Admins", column: "InscriptionNumber", label: "Confidential", informationType: "Admin Inscription Number", rank: "Medium");
        }
    }
}
