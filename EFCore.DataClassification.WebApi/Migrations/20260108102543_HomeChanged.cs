using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class HomeChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YearBuilt",
                table: "Homes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.DropDataClassification(table: "Homes", column: "Size");

            migrationBuilder.AddDataClassification(table: "Homes", column: "Size", label: "Property", informationType: "Home SIZE", rank: "Low");

            migrationBuilder.DropDataClassification(table: "Homes", column: "Price");

            migrationBuilder.DropDataClassification(table: "Homes", column: "OwnerName");

            migrationBuilder.AddDataClassification(table: "Homes", column: "OwnerName", label: "Prive", informationType: "Home Owner Name", rank: "Medium");

            migrationBuilder.DropDataClassification(table: "Homes", column: "Address");

            migrationBuilder.AddDataClassification(table: "Homes", column: "Address", label: "Location", informationType: "Home Address", rank: "Low");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YearBuilt",
                table: "Homes");

            migrationBuilder.DropDataClassification(table: "Homes", column: "Size");

            migrationBuilder.AddDataClassification(table: "Homes", column: "Size", label: "Property", informationType: "Home Size", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Homes", column: "Price", label: "Financial", informationType: "Home Price", rank: "High");

            migrationBuilder.DropDataClassification(table: "Homes", column: "OwnerName");

            migrationBuilder.AddDataClassification(table: "Homes", column: "OwnerName", label: "Personal", informationType: "Home Owner Name", rank: "Medium");

            migrationBuilder.DropDataClassification(table: "Homes", column: "Address");

            migrationBuilder.AddDataClassification(table: "Homes", column: "Address", label: "Location", informationType: "Home Address", rank: "High");
        }
    }
}
