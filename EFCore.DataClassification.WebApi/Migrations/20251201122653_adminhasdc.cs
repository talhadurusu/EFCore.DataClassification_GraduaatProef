using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class adminhasdc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Name"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Name",
                Label = "Confidential",
                InformationType = "Admin Name",
                Rank = "Medium",
                PropertyDisplayName = "Admin.Name"
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
                Label = "Highly Confidential",
                InformationType = "Admin Key",
                Rank = "Critical",
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
                Column = "Name"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Name",
                Label = null,
                InformationType = null,
                Rank = null,
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Admin (Dictionary<string, object>).Name"
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
                Label = null,
                InformationType = null,
                Rank = null,
                PropertyDisplayName = "EFCore.DataClassification.WebApi.Models.Admin (Dictionary<string, object>).Adminkey"
            })
            ;
        }
    }
}
