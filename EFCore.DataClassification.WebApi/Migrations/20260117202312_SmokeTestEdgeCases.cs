using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SmokeTestEdgeCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShadowSecret",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.AddDataClassification(table: "Users", column: "ShadowSecret", label: "Security", informationType: "Shadow Secret", rank: "High");

            migrationBuilder.AddDataClassification(table: "Contractors", column: "AgencyName", label: "Employment", informationType: "Agency Name", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Employees", column: "EmployeeCode", label: "Employment", informationType: "Employee Code", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Employees", column: "Salary", label: "Financial", informationType: "Salary", rank: "High");

            migrationBuilder.AddDataClassification(table: "People", column: "ContactEmail", label: "Contact", informationType: "Email Address", rank: "High");

            migrationBuilder.AddDataClassification(table: "People", column: "ContactPhone", label: "Contact", informationType: "Phone Number", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "People", column: "FullName", label: "Personal", informationType: "Full Name", rank: "Medium");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contractors");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropDataClassification(table: "Users", column: "ShadowSecret");

            migrationBuilder.DropColumn(
                name: "ShadowSecret",
                table: "Users");
        }
    }
}
