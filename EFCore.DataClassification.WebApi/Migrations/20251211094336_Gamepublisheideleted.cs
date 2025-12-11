using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class Gamepublisheideleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublisherUnikeUnitID",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "PublisherUnikeUnitID",
                Label = "Very Confidential",
                InformationType = "Publisher Unique Unit ID",
                Rank = "Medium",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Game (Dictionary<string, object>).PublisherUnikeUnitID"
            })
            ;
        }
    }
}
