using EFCore.DataClassification.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class renameDCchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropDataClassification(
                table: "Admins",
                column: "Phone"
            );

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Admins",
                newName: "Phoneno");

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "Phoneno",
                label: "Confidential",
                informationType: "Phone Number",
                rank: "Low"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropDataClassification(
                table: "Admins",
                column: "Phoneno"
            );

            migrationBuilder.RenameColumn(
                name: "Phoneno",
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
    }
}
