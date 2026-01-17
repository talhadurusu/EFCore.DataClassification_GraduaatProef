using System;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SmokeTest_AllEdgeCases_WithoutDiffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Surname",
                table: "Users");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Documents",
                Column = "Author"
            })
            ;

            migrationBuilder.DropColumn(
                name: "Author",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Adress"
            })
            ;

            migrationBuilder.RenameColumn(
                name: "Adress",
                table: "Users",
                newName: "Address");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Address",
                Label = "Location",
                InformationType = "Home Address",
                Rank = "Low",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Documents",
                Column = "InternalRef"
            })
            ;

            migrationBuilder.RenameColumn(
                name: "InternalRef",
                table: "Documents",
                newName: "Reviewer");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Documents",
                Column = "Reviewer",
                Label = "Docs",
                InformationType = "Reviewer",
                Rank = "Low",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Admins",
                Column = "Adminkey"
            })
            ;

            migrationBuilder.RenameColumn(
                name: "Adminkey",
                table: "Admins",
                newName: "AdminKeyCode");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Admins",
                Column = "AdminKeyCode",
                Label = "Confidential",
                InformationType = "Admin Key",
                Rank = "High",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Salary"
            })
            ;

            migrationBuilder.AlterColumn<int>(
                name: "Salary",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Salary",
                Label = "Confidential",
                InformationType = "Financial Information",
                Rank = "Medium",
            })
            ;

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

            migrationBuilder.AlterColumn<int>(
                name: "AdminId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPasswordChangeUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "LastPasswordChangeUtc",
                Label = "Security",
                InformationType = "Last Password Change",
                Rank = "Medium",
            })
            ;

            migrationBuilder.AddColumn<string>(
                name: "ShadowSecret",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "ShadowSecret",
                Label = "Security",
                InformationType = "Shadow Secret",
                Rank = "High",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Bikes",
                Column = "GearCount"
            })
            ;

            migrationBuilder.AlterColumn<int>(
                name: "GearCount",
                table: "Bikes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Bikes",
                Column = "GearCount",
                Label = "Public",
                InformationType = "Bike Gear Count",
                Rank = "Medium",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Bikes",
                Column = "Brand"
            })
            ;

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "Bikes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Admins",
                Column = "Email",
                Label = "Contact",
                InformationType = "Admin Email",
                Rank = "Medium",
            })
            ;

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                });

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "People",
                Column = "FullName",
                Label = "Personal",
                InformationType = "Full Name",
                Rank = "Medium",
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "People",
                Column = "ContactEmail",
                Label = "Contact",
                InformationType = "Email Address",
                Rank = "High",
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "People",
                Column = "ContactPhone",
                Label = "Contact",
                InformationType = "Phone Number",
                Rank = "Medium",
            })
            ;

            migrationBuilder.CreateTable(
                name: "Contractors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AgencyName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contractors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contractors_People_Id",
                        column: x => x.Id,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Contractors",
                Column = "AgencyName",
                Label = "Employment",
                InformationType = "Agency Name",
                Rank = "Medium",
            })
            ;

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_People_Id",
                        column: x => x.Id,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Employees",
                Column = "EmployeeCode",
                Label = "Employment",
                InformationType = "Employee Code",
                Rank = "Low",
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Employees",
                Column = "Salary",
                Label = "Financial",
                InformationType = "Salary",
                Rank = "High",
            })
            ;

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Contractors");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "LastPasswordChangeUtc"
            })
            ;

            migrationBuilder.DropColumn(
                name: "LastPasswordChangeUtc",
                table: "Users");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "ShadowSecret"
            })
            ;

            migrationBuilder.DropColumn(
                name: "ShadowSecret",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Users",
                newName: "Name");

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Address"
            })
            ;

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Users",
                newName: "Adress");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Adress",
                Label = "Private",
                InformationType = "Home Address",
                Rank = "Medium",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Documents",
                Column = "Reviewer"
            })
            ;

            migrationBuilder.RenameColumn(
                name: "Reviewer",
                table: "Documents",
                newName: "InternalRef");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Documents",
                Column = "InternalRef",
                Label = "Docs",
                InformationType = "InternalRef",
                Rank = "High",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Admins",
                Column = "AdminKeyCode"
            })
            ;

            migrationBuilder.RenameColumn(
                name: "AdminKeyCode",
                table: "Admins",
                newName: "Adminkey");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Admins",
                Column = "Adminkey",
                Label = "Highly Confidential",
                InformationType = "Admin Sleutel",
                Rank = "Critical",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Salary"
            })
            ;

            migrationBuilder.AlterColumn<int>(
                name: "Salary",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Salary",
                Label = "Confidential",
                InformationType = "Financial Information",
                Rank = "High",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Users",
                Column = "Email"
            })
            ;

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AdminId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Surname",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Documents",
                Column = "Author",
                Label = "Docs",
                InformationType = "Yazar",
                Rank = "Medium",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Bikes",
                Column = "GearCount"
            })
            ;

            migrationBuilder.AlterColumn<int>(
                name: "GearCount",
                table: "Bikes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Bikes",
                Column = "GearCount",
                Label = "Public",
                InformationType = "Bike Gear Count",
                Rank = "High",
            })
            ;

            migrationBuilder.AlterColumn<string>(
                name: "Brand",
                table: "Bikes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Bikes",
                Column = "Brand",
                Label = "Internal",
                InformationType = "Bike Brand",
                Rank = "Low",
            })
            ;

            migrationBuilder.Operations.Add(new RemoveDataClassificationOperation
            {
                Schema = "dbo",
                Table = "Admins",
                Column = "Email"
            })
            ;

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Admins_AdminId",
                table: "Users",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id");
        }
    }
}
