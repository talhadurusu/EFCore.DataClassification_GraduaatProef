using EFCore.DataClassification.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class HomeTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropDataClassification(
                table: "Homes",
                column: "Price"
            );

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Homes");

            migrationBuilder.DropDataClassification(
                table: "Homes",
                column: "homeadress"
            );

            migrationBuilder.RenameColumn(
                name: "homeadress",
                table: "Homes",
                newName: "Evadress");

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "YearBuilt",
                label: "public",
                informationType: "Year Built",
                rank: "Low"
            );

            migrationBuilder.DropDataClassification(
                table: "Homes",
                column: "Size"
            );

            migrationBuilder.DropDataClassification(
                table: "Homes",
                column: "OwnerName"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "OwnerName",
                label: "public",
                informationType: "Owner Name",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "Evadress",
                label: "Location",
                informationType: "Home Address Updated",
                rank: "High"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropDataClassification(
                table: "Homes",
                column: "Evadress"
            );

            migrationBuilder.RenameColumn(
                name: "Evadress",
                table: "Homes",
                newName: "homeadress");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Homes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.DropDataClassification(
                table: "Homes",
                column: "YearBuilt"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "Size",
                label: "Property",
                informationType: "Home Size",
                rank: "Low"
            );

            migrationBuilder.DropDataClassification(
                table: "Homes",
                column: "OwnerName"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "OwnerName",
                label: "Personal",
                informationType: "Owner Name",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "homeadress",
                label: "Location",
                informationType: "Home Address Updated",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "Price",
                label: "Financial",
                informationType: "Home Price",
                rank: "Critical"
            );
        }
    }
}
