using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SmokeTest_Phase2_RealChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropDataClassification(table: "Users", column: "Salary");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Users");

            migrationBuilder.DropDataClassification(table: "Games", column: "Genre");

            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Games");

            migrationBuilder.DropDataClassification(table: "Users", column: "Status");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Users",
                newName: "UserName");

            migrationBuilder.RenameColumn(
                name: "LastLoginUtc",
                table: "Users",
                newName: "LastPasswordChangeUtc");

            migrationBuilder.DropDataClassification(table: "Users", column: "FullName");

            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Users",
                newName: "AccountStatus");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "Games",
                newName: "ReleaseYear");

            migrationBuilder.DropDataClassification(table: "Games", column: "PublisherID");

            migrationBuilder.RenameColumn(
                name: "PublisherID",
                table: "Games",
                newName: "Category");

            migrationBuilder.DropDataClassification(table: "Games", column: "Developer");

            migrationBuilder.RenameColumn(
                name: "Developer",
                table: "Games",
                newName: "Studio");

            migrationBuilder.DropDataClassification(table: "Users", column: "Email");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.DropDataClassification(table: "Games", column: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsMultiplayer",
                table: "Games",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddDataClassification(table: "Users", column: "Email", label: "Contact", informationType: "Email Address", rank: "Medium");

            migrationBuilder.DropDataClassification(table: "Users", column: "Adress");

            migrationBuilder.DropDataClassification(table: "Users", column: "AdminId");

            migrationBuilder.AddDataClassification(table: "Users", column: "AdminId", label: "Confidential", informationType: "Admin Reference", rank: "Critical");

            migrationBuilder.AddDataClassification(table: "Users", column: "UserName", label: "Personal", informationType: "User Name", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Users", column: "LastPasswordChangeUtc", label: "Security", informationType: "Last Password Change", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Users", column: "AccountStatus", label: "Internal", informationType: "User Status", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Games", column: "Title", label: "Public", informationType: "Game Title", rank: "None");

            migrationBuilder.DropDataClassification(table: "Games", column: "Price");

            migrationBuilder.AddDataClassification(table: "Games", column: "Price", label: "Financial", informationType: "Game Price", rank: "High");

            migrationBuilder.AddDataClassification(table: "Games", column: "ReleaseYear", label: "Public", informationType: "Release Year", rank: "None");

            migrationBuilder.AddDataClassification(table: "Games", column: "Category", label: "Public", informationType: "Game Category", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Games", column: "Studio", label: "Internal", informationType: "Game Studio", rank: "Medium");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsMultiplayer",
                table: "Games");

            migrationBuilder.DropDataClassification(table: "Users", column: "UserName");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "LastPasswordChangeUtc",
                table: "Users",
                newName: "LastLoginUtc");

            migrationBuilder.DropDataClassification(table: "Users", column: "AccountStatus");

            migrationBuilder.RenameColumn(
                name: "AccountStatus",
                table: "Users",
                newName: "FullName");

            migrationBuilder.DropDataClassification(table: "Games", column: "Studio");

            migrationBuilder.RenameColumn(
                name: "Studio",
                table: "Games",
                newName: "Developer");

            migrationBuilder.RenameColumn(
                name: "ReleaseYear",
                table: "Games",
                newName: "Rating");

            migrationBuilder.DropDataClassification(table: "Games", column: "Category");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Games",
                newName: "PublisherID");

            migrationBuilder.DropDataClassification(table: "Users", column: "Email");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Users",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Genre",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddDataClassification(table: "Users", column: "Email", label: "Contact", informationType: "Email Address", rank: "High");

            migrationBuilder.AddDataClassification(table: "Users", column: "Adress", label: "Private", informationType: "Home Address", rank: "High");

            migrationBuilder.DropDataClassification(table: "Users", column: "AdminId");

            migrationBuilder.AddDataClassification(table: "Users", column: "AdminId", label: "Confidential", informationType: "Admin Reference", rank: "High");

            migrationBuilder.AddDataClassification(table: "Users", column: "Status", label: "Internal", informationType: "User Status", rank: "Low");

            migrationBuilder.DropDataClassification(table: "Users", column: "LastLoginUtc");

            migrationBuilder.AddDataClassification(table: "Users", column: "FullName", label: "Personal", informationType: "User Full Name", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Users", column: "Salary", label: "Confidential", informationType: "Financial Information", rank: "Critical");

            migrationBuilder.DropDataClassification(table: "Games", column: "Title");

            migrationBuilder.DropDataClassification(table: "Games", column: "Price");

            migrationBuilder.AddDataClassification(table: "Games", column: "Price", label: "Financial", informationType: "Game Price", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Games", column: "Description", label: "Public", informationType: "Game Description", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Games", column: "Developer", label: "Internal", informationType: "Developer Name", rank: "Low");

            migrationBuilder.DropDataClassification(table: "Games", column: "Rating");

            migrationBuilder.AddDataClassification(table: "Games", column: "PublisherID", label: "Public", informationType: "Publisher ID", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Games", column: "Genre", label: "Public", informationType: "Game Genre", rank: "None");
        }
    }
}
