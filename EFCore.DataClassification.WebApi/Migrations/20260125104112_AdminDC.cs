using EFCore.DataClassification.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AdminDC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "Notes",
                label: "Internal",
                informationType: "Notes about the admin",
                rank: "Low"
            );

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "InscriptionNo",
                label: "Confidential",
                informationType: "Inscription Number",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "Email",
                label: "Confidential",
                informationType: "Email Address",
                rank: "Medium"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropDataClassification(
                table: "Admins",
                column: "Notes"
            );

            migrationBuilder.DropDataClassification(
                table: "Admins",
                column: "InscriptionNo"
            );

            migrationBuilder.DropDataClassification(
                table: "Admins",
                column: "Email"
            );
        }
    }
}
