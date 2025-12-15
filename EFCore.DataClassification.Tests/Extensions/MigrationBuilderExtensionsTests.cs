using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace EFCore.DataClassification.Tests.Extensions;

/// <summary>
/// Tests for MigrationBuilder extensions
/// </summary>
public class MigrationBuilderExtensionsTests
{
    [Fact]
    public void AddDataClassification_AddsOperationToMigrationBuilder()
    {
        // Arrange
        var migrationBuilder = new MigrationBuilder("SqlServer");

        // Act
        migrationBuilder.AddDataClassification(
            table: "Users",
            column: "Email",
            schema: "dbo",
            label: "Confidential",
            informationType: "Email Address",
            rank: "High"
        );

        // Assert
        var operation = Assert.Single(migrationBuilder.Operations.OfType<CreateDataClassificationOperation>());
        Assert.Equal("Users", operation.Table);
        Assert.Equal("Email", operation.Column);
        Assert.Equal("dbo", operation.Schema);
        Assert.Equal("Confidential", operation.Label);
        Assert.Equal("Email Address", operation.InformationType);
        Assert.Equal("High", operation.Rank);
    }

    [Fact]
    public void AddDataClassification_WithMinimalParameters_AddsOperation()
    {
        // Arrange
        var migrationBuilder = new MigrationBuilder("SqlServer");

        // Act
        migrationBuilder.AddDataClassification(
            table: "Users",
            column: "Email"
        );

        // Assert
        var operation = Assert.Single(migrationBuilder.Operations.OfType<CreateDataClassificationOperation>());
        Assert.Equal("Users", operation.Table);
        Assert.Equal("Email", operation.Column);
        Assert.Null(operation.Schema);
        Assert.Null(operation.Label);
        Assert.Null(operation.InformationType);
        Assert.Null(operation.Rank);
    }

    [Fact]
    public void AddDataClassification_ReturnsOperationBuilder()
    {
        // Arrange
        var migrationBuilder = new MigrationBuilder("SqlServer");

        // Act
        var builder = migrationBuilder.AddDataClassification(
            table: "Users",
            column: "Email"
        );

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<Microsoft.EntityFrameworkCore.Migrations.Operations.Builders.OperationBuilder<CreateDataClassificationOperation>>(builder);
    }

    [Fact]
    public void DropDataClassification_AddsOperationToMigrationBuilder()
    {
        // Arrange
        var migrationBuilder = new MigrationBuilder("SqlServer");

        // Act
        migrationBuilder.DropDataClassification(
            table: "Users",
            column: "Email",
            schema: "dbo"
        );

        // Assert
        var operation = Assert.Single(migrationBuilder.Operations.OfType<RemoveDataClassificationOperation>());
        Assert.Equal("Users", operation.Table);
        Assert.Equal("Email", operation.Column);
        Assert.Equal("dbo", operation.Schema);
    }

    [Fact]
    public void DropDataClassification_WithoutSchema_AddsOperation()
    {
        // Arrange
        var migrationBuilder = new MigrationBuilder("SqlServer");

        // Act
        migrationBuilder.DropDataClassification(
            table: "Users",
            column: "Email"
        );

        // Assert
        var operation = Assert.Single(migrationBuilder.Operations.OfType<RemoveDataClassificationOperation>());
        Assert.Equal("Users", operation.Table);
        Assert.Equal("Email", operation.Column);
        Assert.Null(operation.Schema);
    }

    [Fact]
    public void DropDataClassification_ReturnsOperationBuilder()
    {
        // Arrange
        var migrationBuilder = new MigrationBuilder("SqlServer");

        // Act
        var builder = migrationBuilder.DropDataClassification(
            table: "Users",
            column: "Email"
        );

        // Assert
        Assert.NotNull(builder);
        Assert.IsType<Microsoft.EntityFrameworkCore.Migrations.Operations.Builders.OperationBuilder<RemoveDataClassificationOperation>>(builder);
    }

    [Fact]
    public void MultipleOperations_CanBeAddedToSameMigration()
    {
        // Arrange
        var migrationBuilder = new MigrationBuilder("SqlServer");

        // Act
        migrationBuilder.AddDataClassification("Users", "Email", label: "Confidential");
        migrationBuilder.AddDataClassification("Users", "Phone", label: "Internal");
        migrationBuilder.DropDataClassification("Products", "OldColumn");

        // Assert
        Assert.Equal(2, migrationBuilder.Operations.OfType<CreateDataClassificationOperation>().Count());
        Assert.Single(migrationBuilder.Operations.OfType<RemoveDataClassificationOperation>());
        Assert.Equal(3, migrationBuilder.Operations.Count);
    }

    [Fact]
    public void AddDataClassification_WithNullOptionalParameters_AddsOperation()
    {
        // Arrange
        var migrationBuilder = new MigrationBuilder("SqlServer");

        // Act
        migrationBuilder.AddDataClassification(
            table: "Users",
            column: "Email",
            schema: null,
            label: null,
            informationType: null,
            rank: null
        );

        // Assert
        var operation = Assert.Single(migrationBuilder.Operations.OfType<CreateDataClassificationOperation>());
        Assert.Null(operation.Schema);
        Assert.Null(operation.Label);
        Assert.Null(operation.InformationType);
        Assert.Null(operation.Rank);
    }
}








