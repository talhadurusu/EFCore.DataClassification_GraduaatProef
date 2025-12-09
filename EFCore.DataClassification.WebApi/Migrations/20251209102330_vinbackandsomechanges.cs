using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class vinbackandsomechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VIN",
                table: "Car",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Model"
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

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Maker"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Maker",
                Label = "Public",
                InformationType = "araba yapici",
                Rank = "High",
                PropertyDisplayName = "Car.Maker"
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
            migrationBuilder.DropColumn(
                name: "VIN",
                table: "Car");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "VIN"
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Model"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Model",
                Label = null,
                InformationType = null,
                Rank = null,
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Car (Dictionary<string, object>).Model"
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Maker"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Maker",
                Label = "Public",
                InformationType = "Car Maker",
                Rank = "Low",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Car (Dictionary<string, object>).Maker"
            })
            ;
        }
    }
}
