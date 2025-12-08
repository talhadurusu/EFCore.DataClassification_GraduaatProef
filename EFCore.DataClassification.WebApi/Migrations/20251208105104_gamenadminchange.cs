using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class gamenadminchange : Migration
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

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Description"
            })
            ;

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

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Email"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Email",
                Label = null,
                InformationType = null,
                Rank = null,
                PropertyDisplayName = "Admin.Email"
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
                Column = "PublisherUnikeUnitID"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "PublisherUnikeUnitID",
                Label = "Confidential",
                InformationType = "Publisher Unique Unit ID",
                Rank = "Medium",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Game (Dictionary<string, object>).PublisherUnikeUnitID"
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Description"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Description",
                Label = "Confidential",
                InformationType = "Game Description",
                Rank = "Low",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Game (Dictionary<string, object>).Description"
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Email"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Email",
                Label = "Confidential",
                InformationType = "Email Address",
                Rank = "High",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Admin (Dictionary<string, object>).Email"
            })
            ;
        }
    }
}
