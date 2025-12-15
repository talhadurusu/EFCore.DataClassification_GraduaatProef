using System;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFCore.DataClassification.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adminkey = table.Column<int>(type: "int", nullable: false)
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
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GearCount = table.Column<int>(type: "int", nullable: false)
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
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    VIN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UniqueId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Car", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Homes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salary = table.Column<int>(type: "int", nullable: false),
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
                    PublisherUnikeUnitID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Genre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Adminkey",
                Label = "Highly Confidential",
                InformationType = "Admin Sleutel",
                Rank = "Critical",
                PropertyDisplayName = "Admin.Adminkey"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Admins",
                Column = "Name",
                Label = "Confidential",
                InformationType = "Admin Naam",
                Rank = "Medium",
                PropertyDisplayName = "Admin.Name"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Bikes",
                Column = "Brand",
                Label = "Public",
                InformationType = "Bike Brand",
                Rank = "Low",
                PropertyDisplayName = "Bike.Brand"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Bikes",
                Column = "Owner",
                Label = "Confidential",
                InformationType = "Bike Owner",
                Rank = "High",
                PropertyDisplayName = "Bike.Owner"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "Model",
                Label = "Public",
                InformationType = "Car model",
                Rank = "None",
                PropertyDisplayName = "Car.Model"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "UniqueId",
                Label = "Confidential",
                InformationType = "Unique Car Identifier",
                Rank = "High",
                PropertyDisplayName = "Car.UniqueId"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Car",
                Column = "VIN",
                Label = "Confidential",
                InformationType = "Vehicle Identification Number",
                Rank = "Critical",
                PropertyDisplayName = "Car.VIN"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Description",
                Label = "Confidential",
                InformationType = "Game Story",
                Rank = "Low",
                PropertyDisplayName = "Game.Description"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "Genre",
                Label = "Public",
                InformationType = "Game Genre",
                Rank = "Low",
                PropertyDisplayName = "Game.Genre"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "PublisherUnikeUnitID",
                Label = "Very Confidential",
                InformationType = "Publisher Unique Unit ID",
                Rank = "Medium",
                PropertyDisplayName = "Game.PublisherUnikeUnitID"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Games",
                Column = "ReleaseDate",
                Label = "Public",
                InformationType = "Release Date",
                Rank = "None",
                PropertyDisplayName = "Game.ReleaseDate"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Homes",
                Column = "Address",
                Label = "Private",
                InformationType = "Home Address",
                Rank = "Medium",
                PropertyDisplayName = "Home.Address"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Homes",
                Column = "OwnerName",
                Label = "Confidential",
                InformationType = "Owner Name",
                Rank = "High",
                PropertyDisplayName = "Home.OwnerName"
            })
            ;

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Homes",
                Column = "Size",
                Label = "Public",
                InformationType = "Home Size",
                Rank = "Low",
                PropertyDisplayName = "Home.Size"
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

            migrationBuilder.Operations.Add(new CreateDataClassificationOperation
            {
                Schema = null,
                Table = "Users",
                Column = "PhoneNumber",
                Label = "Internal",
                InformationType = "Phone Number",
                Rank = "High",
                PropertyDisplayName = "User.PhoneNumber"
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bikes");

            migrationBuilder.DropTable(
                name: "Car");

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
