using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClassificationLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Salary"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Salary",
                Label = "Confidential",
                InformationType = "Financial Information",
                Rank = "High",
                PropertyDisplayName = "User.Salary"
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Adress"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Adress",
                Label = "Private",
                InformationType = "Home Address",
                Rank = "Medium",
                PropertyDisplayName = "User.Adress"
            })
            ;
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Salary"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Salary",
                Label = "blablabla",
                InformationType = "Salary blabla",
                Rank = "High",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.User (Dictionary<string, object>).Salary"
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Adress"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Adress",
                Label = "Private",
                InformationType = "ev adresi",
                Rank = "Medium",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.User (Dictionary<string, object>).Adress"
            })
            ;
        }
    }
}
