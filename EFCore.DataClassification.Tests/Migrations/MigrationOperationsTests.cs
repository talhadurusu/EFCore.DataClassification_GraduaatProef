using EFCore.DataClassification.Operations;
using Xunit;

namespace EFCore.DataClassification.Tests.Migrations;

/// <summary>
/// Tests for custom migration operations
/// </summary>
public class MigrationOperationsTests
{
    [Fact]
    public void CreateDataClassificationOperation_SetsProperties()
    {
        // Arrange & Act
        var operation = new CreateDataClassificationOperation
        {
            Schema = "dbo",
            Table = "Users",
            Column = "Email",
            Label = "Confidential",
            InformationType = "Email Address",
            Rank = "High",
            PropertyDisplayName = "User.Email"
        };

        // Assert
        Assert.Equal("dbo", operation.Schema);
        Assert.Equal("Users", operation.Table);
        Assert.Equal("Email", operation.Column);
        Assert.Equal("Confidential", operation.Label);
        Assert.Equal("Email Address", operation.InformationType);
        Assert.Equal("High", operation.Rank);
        Assert.Equal("User.Email", operation.PropertyDisplayName);
    }

    [Fact]
    public void RemoveDataClassificationOperation_SetsProperties()
    {
        // Arrange & Act
        var operation = new RemoveDataClassificationOperation
        {
            Schema = "dbo",
            Table = "Users",
            Column = "Email"
        };

        // Assert
        Assert.Equal("dbo", operation.Schema);
        Assert.Equal("Users", operation.Table);
        Assert.Equal("Email", operation.Column);
    }

    [Fact]
    public void CreateDataClassificationOperation_HandlesNullSchema()
    {
        // Arrange & Act
        var operation = new CreateDataClassificationOperation
        {
            Schema = null,
            Table = "Users",
            Column = "Email",
            Label = "Test",
            InformationType = "Test",
            Rank = "Low"
        };

        // Assert
        Assert.Null(operation.Schema);
        Assert.Equal("Users", operation.Table);
    }

    [Fact]
    public void CreateDataClassificationOperation_HandlesNullValues()
    {
        // Arrange & Act
        var operation = new CreateDataClassificationOperation
        {
            Schema = "dbo",
            Table = "Users",
            Column = "Email",
            Label = null,
            InformationType = null,
            Rank = null
        };

        // Assert
        Assert.Null(operation.Label);
        Assert.Null(operation.InformationType);
        Assert.Null(operation.Rank);
    }
}


