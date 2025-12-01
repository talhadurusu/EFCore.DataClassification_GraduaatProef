using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminIdDataClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "AdminId"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "AdminId",
                Label = "Confidential",
                InformationType = "Admin Reference",
                Rank = "High",
                PropertyDisplayName = "User.AdminId"
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
                Column = "AdminId"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "AdminId",
                Label = "Confidential",
                InformationType = "Admin Id",
                Rank = "High",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.User (Dictionary<string, object>).AdminId"
            })
            ;
        }
    }
}
