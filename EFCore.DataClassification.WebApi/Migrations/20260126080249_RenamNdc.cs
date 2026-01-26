using EFCore.DataClassification.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class RenamNdc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNo",
                table: "Admins",
                newName: "Phone");

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "Phone",
                label: "Confidential",
                informationType: "Phone Number",
                rank: "Medium"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropDataClassification(
                table: "Admins",
                column: "Phone"
            );

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Admins",
                newName: "PhoneNo");
        }
    }
}
