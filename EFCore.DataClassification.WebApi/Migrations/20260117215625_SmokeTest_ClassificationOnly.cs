using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SmokeTest_ClassificationOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Email"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Email",
                Label = "Confidential",
                InformationType = "User Email",
                Rank = "Medium",
            })
            ;
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Email"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Email",
                Label = "Contact",
                InformationType = "Email Address",
                Rank = "High",
            })
            ;
        }
    }
}
