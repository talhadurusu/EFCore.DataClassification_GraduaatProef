using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGenreClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Genre"
            })
            ;
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Genre",
                Label = "Public",
                InformationType = "Game Genre",
                Rank = "Low",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Game (Dictionary<string, object>).Genre"
            })
            ;
        }
    }
}
