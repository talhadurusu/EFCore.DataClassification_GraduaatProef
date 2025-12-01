using EFCore.DataClassification.Exceptions;
using EFCore.DataClassification.Infrastructure;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Xunit;

namespace EFCore.DataClassification.Tests.SqlGenerator;

public class DataClassificationSqlGeneratorTests
{
    [Fact]
    public void Generate_CreateDataClassificationOperation_ProducesSqlWithExtendedPropertiesAndSensitivity()
    {
        // Arrange
        var operation = new CreateDataClassificationOperation
        {
            Schema = "dbo",
            Table = "Users",
            Column = "Email",
            Label = "Confidential",
            InformationType = "Email Address",
            Rank = "High"
        };

        var sql = GenerateSql(operation);

        // Assert
        Assert.Contains("sp_addextendedproperty", sql);
        Assert.Contains("DataClassification:Label", sql);
        Assert.Contains("DataClassification:InformationType", sql);
        Assert.Contains("DataClassification:Rank", sql);
        Assert.Contains("ADD SENSITIVITY CLASSIFICATION", sql);
        Assert.Contains("LABEL = N'Confidential'", sql);
        Assert.Contains("INFORMATION_TYPE = N'Email Address'", sql);
        Assert.Contains("RANK = HIGH", sql);
    }

    [Fact]
    public void Generate_RemoveDataClassificationOperation_ProducesSqlWithDropStatements()
    {
        // Arrange
        var operation = new RemoveDataClassificationOperation
        {
            Schema = "dbo",
            Table = "Users",
            Column = "Email"
        };

        var sql = GenerateSql(operation);

        // Assert
        Assert.Contains("sp_dropextendedproperty", sql);
        Assert.Contains("DROP SENSITIVITY CLASSIFICATION", sql);
        Assert.Contains("[dbo].[Users]", sql);
    }

    [Fact]
    public void Generate_DropColumnWithClassification_ClearsMetadataFirst()
    {
        // Arrange
        var operation = new DropColumnOperation
        {
            Schema = "dbo",
            Table = "Users",
            Name = "Email"
        };

        var sql = GenerateSql(operation);

        // Assert
        var dropExtendedIndex = sql.IndexOf("sp_dropextendedproperty");
        var dropSensitivityIndex = sql.IndexOf("DROP SENSITIVITY CLASSIFICATION");
        var dropColumnIndex = sql.IndexOf("DROP COLUMN");

        Assert.True(dropExtendedIndex >= 0, "Should contain sp_dropextendedproperty");
        Assert.True(dropSensitivityIndex >= 0, "Should contain DROP SENSITIVITY CLASSIFICATION");
        Assert.True(dropColumnIndex >= 0, "Should contain DROP COLUMN");
        Assert.True(dropExtendedIndex < dropColumnIndex, "Extended properties should be dropped before column");
        Assert.True(dropSensitivityIndex < dropColumnIndex, "Sensitivity classification should be dropped before column");
    }

    [Fact]
    public void Validation_InvalidRank_ThrowsException()
    {
        // Arrange
        var operation = new CreateDataClassificationOperation
        {
            Schema = "dbo",
            Table = "Users",
            Column = "Email",
            Label = "Test",
            InformationType = "Test",
            Rank = "InvalidRank"
        };

        // Act & Assert
        Assert.Throws<DataClassificationException>(() => GenerateSql(operation));
    }

    [Fact]
    public void Validation_LabelTooLong_ThrowsException()
    {
        // Arrange
        var operation = new CreateDataClassificationOperation
        {
            Schema = "dbo",
            Table = "Users",
            Column = "Email",
            Label = new string('A', 129), // 129 characters
            InformationType = "Test",
            Rank = "Low"
        };

        // Act & Assert
        Assert.Throws<DataClassificationException>(() => GenerateSql(operation));
    }

    [Fact]
    public void Generate_NoneRank_ProducesSqlWithoutRank()
    {
        // Arrange
        var operation = new CreateDataClassificationOperation
        {
            Schema = "dbo",
            Table = "Users",
            Column = "Email",
            Label = "Public",
            InformationType = "Display Name",
            Rank = "None"
        };

        var sql = GenerateSql(operation);

        // Assert
        Assert.Contains("ADD SENSITIVITY CLASSIFICATION", sql);
        Assert.DoesNotContain("RANK =", sql); // None should not produce RANK clause
    }

    [Fact]
    public void Generate_EmptyClassification_ProducesNoSql()
    {
        // Arrange
        var operation = new CreateDataClassificationOperation
        {
            Schema = "dbo",
            Table = "Users",
            Column = "Email",
            Label = null,
            InformationType = null,
            Rank = null
        };

        var sql = GenerateSql(operation);

        // Assert - Should not generate any classification SQL
        Assert.DoesNotContain("sp_addextendedproperty", sql);
        Assert.DoesNotContain("ADD SENSITIVITY CLASSIFICATION", sql);
    }

    #region Helper Methods

    private string GenerateSql(MigrationOperation operation)
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlServer("Server=.;Database=Test;")
            .Options;

        using var context = new DbContext(options);
        var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;
        
        var dependencies = serviceProvider.GetService(typeof(MigrationsSqlGeneratorDependencies)) as MigrationsSqlGeneratorDependencies;
        var commandBatchPreparer = serviceProvider.GetService(typeof(ICommandBatchPreparer)) as ICommandBatchPreparer;

        if (dependencies == null || commandBatchPreparer == null)
            throw new InvalidOperationException("Could not resolve required services");

        var generator = new DataClassificationSqlGenerator(dependencies, commandBatchPreparer);
        
        var commands = generator.Generate(new[] { operation }, null);
        var sql = string.Join("\n", commands.Select(c => c.CommandText));

        return sql;
    }

    #endregion
}
