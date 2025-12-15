using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class Dcdeletedandchanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "ReleaseDate"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "ReleaseDate",
                Label = "Intern",
                InformationType = "Release Date",
                Rank = "None",
                PropertyDisplayName = "Game.ReleaseDate"
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Description"
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
                Column = "ReleaseDate"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "ReleaseDate",
                Label = "Public",
                InformationType = "Release Date",
                Rank = "None",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Game (Dictionary<string, object>).ReleaseDate"
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
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Game (Dictionary<string, object>).Description"
            })
            ;
        }
    }
}
