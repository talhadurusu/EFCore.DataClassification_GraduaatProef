using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class adminNoDc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Adminkey"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Adminkey",
                Label = null,
                InformationType = null,
                Rank = null,
                PropertyDisplayName = "Admin.Adminkey"
            })
            ;
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                Label = "Private",
                InformationType = "Contact Info",
                Rank = "Critical",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Admin (Dictionary<string, object>).Email"
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Adminkey"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Adminkey",
                Label = "Private",
                InformationType = "Security Info",
                Rank = "Critical",
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Admin (Dictionary<string, object>).Adminkey"
            })
            ;
        }
    }
}
