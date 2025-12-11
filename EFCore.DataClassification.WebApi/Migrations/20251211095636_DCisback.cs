using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class DCisback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublisherUnikeUnitID",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Car",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VIN",
                table: "Car",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Description",
                Label = "Confidential",
                InformationType = "Game Story",
                Rank = "Low",
                PropertyDisplayName = "Game.Description"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "PublisherUnikeUnitID",
                Label = "Very Confidential",
                InformationType = "Publisher Unique Unit ID",
                Rank = "Medium",
                PropertyDisplayName = "Game.PublisherUnikeUnitID"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Model",
                Label = "Public",
                InformationType = "Car model",
                Rank = "None",
                PropertyDisplayName = "Car.Model"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "VIN",
                Label = "Confidential",
                InformationType = "Vehicle Identification Number",
                Rank = "Critical",
                PropertyDisplayName = "Car.VIN"
            })
            ;
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Description"
            })
            ;

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Games");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "PublisherUnikeUnitID"
            })
            ;

            migrationBuilder.DropColumn(
                name: "PublisherUnikeUnitID",
                table: "Games");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Model"
            })
            ;

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Car");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "VIN"
            })
            ;

            migrationBuilder.DropColumn(
                name: "VIN",
                table: "Car");
        }
    }
}
