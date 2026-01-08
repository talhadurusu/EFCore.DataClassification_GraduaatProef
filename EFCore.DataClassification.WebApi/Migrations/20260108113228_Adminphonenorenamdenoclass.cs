using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class Adminphonenorenamdenoclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropDataClassification(table: "Admins", column: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Admins",
                newName: "PhoneNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNo",
                table: "Admins",
                newName: "PhoneNumber");

            migrationBuilder.AddDataClassification(table: "Admins", column: "PhoneNumber", label: "Confidential", informationType: "Admin Phone Number", rank: "High");
        }
    }
}
