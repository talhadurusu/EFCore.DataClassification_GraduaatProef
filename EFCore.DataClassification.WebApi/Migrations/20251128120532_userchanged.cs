using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class userchanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Email"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Email",
                Label = null,
                InformationType = null,
                Rank = null,
                PropertyDisplayName = "User.Email"
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
                Column = "Email"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "Email",
                Label = "Public",
                InformationType = "Contact Info",
                Rank = "Critical",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.User (Dictionary<string, object>).Email"
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
                InformationType = "huis adress",
                Rank = "Medium",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.User (Dictionary<string, object>).Adress"
            })
            ;
        }
    }
}
