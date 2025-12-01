using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace EFCore.DataClassification.Tests.Generators;

/// <summary>
/// Tests for DataClassificationMigrationsGenerator
/// These tests verify that the generator includes the necessary namespaces
/// </summary>
public class MigrationsGeneratorTests
{
    [Fact]
    public void DataClassificationOperations_RequireCorrectNamespaces()
    {
        // Arrange - Verify that our operations are in the correct namespace
        var createOp = new CreateDataClassificationOperation
        {
            Table = "Users",
            Column = "Email"
        };

        var removeOp = new RemoveDataClassificationOperation
        {
            Table = "Users",
            Column = "Phone"
        };

        // Assert - Operations should be in the Operations namespace
        Assert.Equal("EFCore.DataClassification.Operations", createOp.GetType().Namespace);
        Assert.Equal("EFCore.DataClassification.Operations", removeOp.GetType().Namespace);
    }

    [Fact]
    public void CreateDataClassificationOperation_IsValidMigrationOperation()
    {
        // Arrange & Act
        var operation = new CreateDataClassificationOperation
        {
            Table = "Users",
            Column = "Email",
            Label = "Confidential"
        };

        // Assert - Should be a valid MigrationOperation
        Assert.IsAssignableFrom<MigrationOperation>(operation);
        Assert.NotNull(operation.Table);
        Assert.NotNull(operation.Column);
    }

    [Fact]
    public void RemoveDataClassificationOperation_IsValidMigrationOperation()
    {
        // Arrange & Act
        var operation = new RemoveDataClassificationOperation
        {
            Table = "Users",
            Column = "Email"
        };

        // Assert - Should be a valid MigrationOperation
        Assert.IsAssignableFrom<MigrationOperation>(operation);
        Assert.NotNull(operation.Table);
        Assert.NotNull(operation.Column);
    }

    [Fact]
    public void MultipleDataClassificationOperations_CanCoexist()
    {
        // Arrange
        var operations = new List<MigrationOperation>
        {
            new CreateDataClassificationOperation { Table = "Users", Column = "Email" },
            new CreateDataClassificationOperation { Table = "Users", Column = "Phone" },
            new RemoveDataClassificationOperation { Table = "Products", Column = "OldColumn" },
            new CreateTableOperation { Name = "NewTable" }
        };

        // Act
        var dataClassificationOps = operations
            .Where(op => op.GetType().Namespace == "EFCore.DataClassification.Operations")
            .ToList();

        // Assert
        Assert.Equal(3, dataClassificationOps.Count);
        Assert.Equal(2, dataClassificationOps.OfType<CreateDataClassificationOperation>().Count());
        Assert.Single(dataClassificationOps.OfType<RemoveDataClassificationOperation>());
    }

    [Fact]
    public void DataClassificationOperations_HaveDistinctTypes()
    {
        // Arrange
        var createOp = new CreateDataClassificationOperation();
        var removeOp = new RemoveDataClassificationOperation();

        // Assert
        Assert.NotEqual(createOp.GetType(), removeOp.GetType());
        Assert.True(createOp is MigrationOperation);
        Assert.True(removeOp is MigrationOperation);
    }
}
