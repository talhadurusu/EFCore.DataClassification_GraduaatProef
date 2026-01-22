using System;
using EFCore.DataClassification.Extensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class FirstCreate : Migration
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
                    PhoneNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InscriptionNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    homeadress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    YearBuilt = table.Column<int>(type: "int", nullable: false)
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
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdminId = table.Column<int>(type: "int", nullable: true),
                    LastPasswordChangeUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Studio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleaseYear = table.Column<int>(type: "int", nullable: false),
                    IsMultiplayer = table.Column<bool>(type: "bit", nullable: false),
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

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "CreatedAtUtc",
                label: "Normal",
                informationType: "Admin Created At",
                rank: "Low"
            );

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "Email",
                label: "Confidential",
                informationType: "Admin Email",
                rank: "Critical"
            );

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "FirstName",
                label: "Normal",
                informationType: "Admin First Name",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "InscriptionNo",
                label: "Confidential",
                informationType: "Admin Inscription Number",
                rank: "Medium"
            );

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "LastName",
                label: "Confidential",
                informationType: " Last Name",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Admins",
                column: "Notes",
                label: "Internal",
                informationType: "Admin Notes",
                rank: "Low"
            );

            migrationBuilder.AddDataClassification(
                table: "Car",
                column: "Model",
                label: "Public",
                informationType: "Car Model Name",
                rank: "Low"
            );

            migrationBuilder.AddDataClassification(
                table: "Car",
                column: "Notes",
                label: "Confidential",
                informationType: "Car Notes",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Car",
                column: "OwnerEmail",
                label: "Confidential",
                informationType: "Car Owner Email",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Car",
                column: "OwnerName",
                label: "Confidential",
                informationType: "Car Owner Name",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Car",
                column: "UniqueId",
                label: "Confidential",
                informationType: "Unique Car Identifier",
                rank: "Medium"
            );

            migrationBuilder.AddDataClassification(
                table: "Car",
                column: "VehicleIdentificationNumber",
                label: "Confidential",
                informationType: "Vehicle Identification Number",
                rank: "Critical"
            );

            migrationBuilder.AddDataClassification(
                table: "Car",
                column: "Year",
                label: "Internal",
                informationType: "Car Manufacturing Year",
                rank: "Low"
            );

            migrationBuilder.AddDataClassification(
                table: "Customers",
                column: "Address",
                label: "Address",
                informationType: "Mailing Address",
                rank: "None"
            );

            migrationBuilder.AddDataClassification(
                table: "Customers",
                column: "Email",
                label: "Contact",
                informationType: "Email Address",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Customers",
                column: "FullName",
                label: "Contact",
                informationType: "Customer Full Name",
                rank: "Medium"
            );

            migrationBuilder.AddDataClassification(
                table: "Customers",
                column: "PhoneNumber",
                label: "Contact",
                informationType: "Phone Number",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Documents",
                column: "Body",
                label: "Docs",
                informationType: "Body",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Documents",
                column: "Summary",
                label: "Docs",
                informationType: "Summary",
                rank: "Low"
            );

            migrationBuilder.AddDataClassification(
                table: "Documents",
                column: "Title",
                label: "Docs",
                informationType: "Title",
                rank: "None"
            );

            migrationBuilder.AddDataClassification(
                table: "Documents",
                column: "Writer",
                label: "Docs",
                informationType: "Writer Name",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Games",
                column: "Category",
                label: "Public",
                informationType: "Game Category",
                rank: "Low"
            );

            migrationBuilder.AddDataClassification(
                table: "Games",
                column: "Price",
                label: "Financial",
                informationType: "Game Price",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Games",
                column: "ReleaseYear",
                label: "Public",
                informationType: "Release Year",
                rank: "None"
            );

            migrationBuilder.AddDataClassification(
                table: "Games",
                column: "Studio",
                label: "Internal",
                informationType: "Game Studio",
                rank: "Medium"
            );

            migrationBuilder.AddDataClassification(
                table: "Games",
                column: "Title",
                label: "Public",
                informationType: "Game Title",
                rank: "None"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "OwnerName",
                label: "Personal",
                informationType: "Owner Name",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "Price",
                label: "Financial",
                informationType: "Home Price",
                rank: "Critical"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "Size",
                label: "Property",
                informationType: "Home Size",
                rank: "Low"
            );

            migrationBuilder.AddDataClassification(
                table: "Homes",
                column: "homeadress",
                label: "Location",
                informationType: "Home Address Updated",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Users",
                column: "AccountStatus",
                label: "Internal",
                informationType: "User Status",
                rank: "Low"
            );

            migrationBuilder.AddDataClassification(
                table: "Users",
                column: "AdminId",
                label: "Confidential",
                informationType: "Admin Reference",
                rank: "Critical"
            );

            migrationBuilder.AddDataClassification(
                table: "Users",
                column: "Email",
                label: "Contact",
                informationType: "Email Address",
                rank: "Medium"
            );

            migrationBuilder.AddDataClassification(
                table: "Users",
                column: "LastPasswordChangeUtc",
                label: "Security",
                informationType: "Last Password Change",
                rank: "Medium"
            );

            migrationBuilder.AddDataClassification(
                table: "Users",
                column: "PhoneNumber",
                label: "Internal",
                informationType: "Phone Number",
                rank: "High"
            );

            migrationBuilder.AddDataClassification(
                table: "Users",
                column: "UserName",
                label: "Personal",
                informationType: "User Name",
                rank: "Low"
            );
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
