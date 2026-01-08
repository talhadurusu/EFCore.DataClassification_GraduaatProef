using System;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class SmokeTest_Comprehensive_AllEdgeCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InscriptionNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FavoriteBookAuthor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GearCount = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bikes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Car",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleIdentificationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UniqueId = table.Column<int>(type: "int", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorPreference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Car", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Writer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Homes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastLoginUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Admins",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublisherID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Developer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_UserId",
                table: "Games",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AdminId",
                table: "Users",
                column: "AdminId");

            migrationBuilder.AddDataClassification(table: "Admins", column: "Email", label: "Confidential", informationType: "Admin Email", rank: "High");

            migrationBuilder.AddDataClassification(table: "Admins", column: "FavoriteBookAuthor", label: "Internal", informationType: "Favorite Author", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Admins", column: "FirstName", label: "Confidential", informationType: "Admin First Name", rank: "High");

            migrationBuilder.AddDataClassification(table: "Admins", column: "InscriptionNumber", label: "Confidential", informationType: "Admin Inscription Number", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Admins", column: "LastName", label: "Confidential", informationType: "Admin Last Name", rank: "High");

            migrationBuilder.AddDataClassification(table: "Admins", column: "Notes", label: "Internal", informationType: "Admin Notes", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Admins", column: "PhoneNumber", label: "Confidential", informationType: "Admin Phone Number", rank: "High");

            migrationBuilder.AddDataClassification(table: "Bikes", column: "SerialNumber", label: "Confidential", informationType: "Bike Serial Number", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Car", column: "Model", label: "Public", informationType: "Car Model Name", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Car", column: "Notes", label: "Confidential", informationType: "Car Notes", rank: "High");

            migrationBuilder.AddDataClassification(table: "Car", column: "OwnerEmail", label: "Confidential", informationType: "Car Owner Email", rank: "High");

            migrationBuilder.AddDataClassification(table: "Car", column: "OwnerName", label: "Confidential", informationType: "Car Owner Name", rank: "High");

            migrationBuilder.AddDataClassification(table: "Car", column: "UniqueId", label: "Confidential", informationType: "Unique Car Identifier", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Car", column: "VehicleIdentificationNumber", label: "Confidential", informationType: "Vehicle Identification Number", rank: "Critical");

            migrationBuilder.AddDataClassification(table: "Car", column: "Year", label: "Internal", informationType: "Car Manufacturing Year", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Customers", column: "Address", label: "Address", informationType: "Mailing Address", rank: "None");

            migrationBuilder.AddDataClassification(table: "Customers", column: "Email", label: "Contact", informationType: "Email Address", rank: "High");

            migrationBuilder.AddDataClassification(table: "Customers", column: "FullName", label: "Contact", informationType: "Customer Full Name", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Customers", column: "PhoneNumber", label: "Contact", informationType: "Phone Number", rank: "High");

            migrationBuilder.AddDataClassification(table: "Documents", column: "Body", label: "Docs", informationType: "Body", rank: "High");

            migrationBuilder.AddDataClassification(table: "Documents", column: "Summary", label: "Docs", informationType: "Summary", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Documents", column: "Title", label: "Docs", informationType: "Title", rank: "None");

            migrationBuilder.AddDataClassification(table: "Documents", column: "Writer", label: "Docs", informationType: "Writer Name", rank: "High");

            migrationBuilder.AddDataClassification(table: "Games", column: "Description", label: "Public", informationType: "Game Description", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Games", column: "Developer", label: "Internal", informationType: "Developer Name", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Games", column: "Genre", label: "Public", informationType: "Game Genre", rank: "None");

            migrationBuilder.AddDataClassification(table: "Games", column: "Price", label: "Financial", informationType: "Game Price", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Games", column: "PublisherID", label: "Public", informationType: "Publisher ID", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Homes", column: "Address", label: "Location", informationType: "Home Address", rank: "High");

            migrationBuilder.AddDataClassification(table: "Homes", column: "OwnerName", label: "Personal", informationType: "Home Owner Name", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Homes", column: "Price", label: "Financial", informationType: "Home Price", rank: "High");

            migrationBuilder.AddDataClassification(table: "Homes", column: "Size", label: "Property", informationType: "Home Size", rank: "Low");

            migrationBuilder.AddDataClassification(table: "Users", column: "AdminId", label: "Confidential", informationType: "Admin Reference", rank: "High");

            migrationBuilder.AddDataClassification(table: "Users", column: "Adress", label: "Private", informationType: "Home Address", rank: "High");

            migrationBuilder.AddDataClassification(table: "Users", column: "Email", label: "Contact", informationType: "Email Address", rank: "High");

            migrationBuilder.AddDataClassification(table: "Users", column: "FullName", label: "Personal", informationType: "User Full Name", rank: "Medium");

            migrationBuilder.AddDataClassification(table: "Users", column: "PhoneNumber", label: "Internal", informationType: "Phone Number", rank: "High");

            migrationBuilder.AddDataClassification(table: "Users", column: "Salary", label: "Confidential", informationType: "Financial Information", rank: "Critical");

            migrationBuilder.AddDataClassification(table: "Users", column: "Status", label: "Internal", informationType: "User Status", rank: "Low");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bikes");

            migrationBuilder.DropTable(
                name: "Car");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Homes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Admins");
        }
    }
}
