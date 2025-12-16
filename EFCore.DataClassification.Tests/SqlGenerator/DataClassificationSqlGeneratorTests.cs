using System;
using System.Collections.Generic;
using System.Linq;
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

public class DataClassificationSqlGeneratorTests {
    [Fact]
    public void Generate_CreateDataClassificationOperation_ProducesSqlWithExtendedPropertiesAndGuardedSensitivity() {
        var operation = new CreateDataClassificationOperation {
            Schema = "dbo",
            Table = "Users",
            Column = "Email",
            Label = "Confidential",
            InformationType = "Email Address",
            Rank = "High"
        };

        var sql = GenerateSql(operation);

        // Extended properties
        Assert.Contains("sp_addextendedproperty", sql);
        Assert.Contains("DataClassification:Label", sql);
        Assert.Contains("DataClassification:InformationType", sql);
        Assert.Contains("DataClassification:Rank", sql);

        // Version guard
        Assert.Contains("SERVERPROPERTY('ProductMajorVersion')", sql);
        Assert.Contains(">= 15", sql);

        // Dynamic SQL execution
        Assert.Contains("sp_executesql", sql);

        // Sensitivity semantics (NOT literal format!)
        Assert.Contains("ADD SENSITIVITY CLASSIFICATION", sql);
        Assert.Contains("LABEL", sql);
        Assert.Contains("Confidential", sql);
        Assert.Contains("INFORMATION_TYPE", sql);
        Assert.Contains("Email Address", sql);
        Assert.Contains("RANK = HIGH", sql);
    }


    [Fact]
    public void Generate_RemoveDataClassificationOperation_ProducesSqlWithDropStatements() {
        // Arrange
        var operation = new RemoveDataClassificationOperation {
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
    public void Generate_DropColumnWithClassification_ClearsMetadataFirst() {
        // Arrange: According to the new architecture, first Remove, then DropColumn
        var removeOp = new RemoveDataClassificationOperation {
            Schema = "dbo",
            Table = "Users",
            Column = "Email"
        };

        var dropColumnOp = new DropColumnOperation {
            Schema = "dbo",
            Table = "Users",
            Name = "Email"
        };

        var sql = GenerateSql(new MigrationOperation[] { removeOp, dropColumnOp });

        // Assert
        var dropExtendedIndex = sql.IndexOf("sp_dropextendedproperty", StringComparison.Ordinal);
        var dropSensitivityIndex = sql.IndexOf("DROP SENSITIVITY CLASSIFICATION", StringComparison.Ordinal);
        var dropColumnIndex = sql.IndexOf("DROP COLUMN", StringComparison.Ordinal);

        Assert.True(dropExtendedIndex >= 0, "Should contain sp_dropextendedproperty");
        Assert.True(dropSensitivityIndex >= 0, "Should contain DROP SENSITIVITY CLASSIFICATION");
        Assert.True(dropColumnIndex >= 0, "Should contain DROP COLUMN");
        Assert.True(dropExtendedIndex < dropColumnIndex, "Extended properties should be dropped before column");
        Assert.True(dropSensitivityIndex < dropColumnIndex, "Sensitivity classification should be dropped before column");
    }

    [Fact]
    public void Validation_InvalidRank_ThrowsException() {
        // Arrange
        var operation = new CreateDataClassificationOperation {
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
    public void Validation_LabelTooLong_ThrowsException() {
        // Arrange
        var operation = new CreateDataClassificationOperation {
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
    public void Generate_NoneRank_ProducesSqlWithoutRank() {
        // Arrange
        var operation = new CreateDataClassificationOperation {
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
    public void Generate_EmptyClassification_ProducesNoSql() {
        // Arrange
        var operation = new CreateDataClassificationOperation {
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

    /// <summary>
    /// Check whether the CREATE TABLE override is overriding the base behaviour.
    /// </summary>
    [Fact]
    public void Generate_CreateTableOperation_CallsBaseWithoutError() {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer("Server=.;Database=Test;")
            .Options;

        IModel model;
        using (var context = new TestDbContext(options)) {
            model = context.Model;
        }

        var createTableOp = new CreateTableOperation {
            Name = "TestEntities",
            Schema = "dbo",
            Columns =
            {
                new AddColumnOperation
                {
                    Name = "Id",
                    Table = "TestEntities",
                    Schema = "dbo",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false
                },
                new AddColumnOperation
                {
                    Name = "Email",
                    Table = "TestEntities",
                    Schema = "dbo",
                    ClrType = typeof(string),
                    ColumnType = "nvarchar(max)",
                    IsNullable = false
                }
            },
            PrimaryKey = new AddPrimaryKeyOperation {
                Name = "PK_TestEntities",
                Table = "TestEntities",
                Schema = "dbo",
                Columns = new[] { "Id" }
            }
        };

        // Act
        var sql = GenerateSql(createTableOp, model);

        // Assert – sadece base davranışını kontrol et
        Assert.Contains("CREATE TABLE", sql);
        Assert.Contains("[dbo].[TestEntities]", sql);
        Assert.Contains("[Id]", sql);
        Assert.Contains("[Email]", sql);
    }

    /// <summary>
    /// Check whether the ADD COLUMN override disrupts the base behaviour.
    /// </summary>
    [Fact]
    public void Generate_AddColumnOperation_CallsBaseWithoutError() {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer("Server=.;Database=Test;")
            .Options;

        IModel model;
        using (var context = new TestDbContext(options)) {
            model = context.Model;
        }

        var addColumnOp = new AddColumnOperation {
            Name = "Email",
            Table = "TestEntities",
            Schema = "dbo",
            ClrType = typeof(string),
            ColumnType = "nvarchar(max)",
            IsNullable = false
        };

        // Act
        var sql = GenerateSql(addColumnOp, model);

        // Assert 
        Assert.Contains("ALTER TABLE", sql);
        Assert.Contains("[dbo].[TestEntities]", sql);
        Assert.Contains("ADD [Email]", sql);
    }

    /// <summary>
    /// The SQL statement CREATE TABLE classification should not be generated in a table without classification.
    /// </summary>
    [Fact]
    public void Generate_CreateTableWithoutClassification_ProducesNoClassificationSql() {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextNoClassification>()
            .UseSqlServer("Server=.;Database=Test;")
            .Options;

        using var context = new TestDbContextNoClassification(options);
        var model = context.Model;

        var createTableOp = new CreateTableOperation {
            Name = "SimpleEntities",
            Schema = "dbo",
            Columns =
            {
                new AddColumnOperation
                {
                    Name = "Id",
                    Table = "SimpleEntities",
                    Schema = "dbo",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false
                },
                new AddColumnOperation
                {
                    Name = "Name",
                    Table = "SimpleEntities",
                    Schema = "dbo",
                    ClrType = typeof(string),
                    ColumnType = "nvarchar(max)",
                    IsNullable = false
                }
            }
        };

        // Act
        var sql = GenerateSql(createTableOp, model);

        // Assert
        Assert.Contains("CREATE TABLE", sql);
        Assert.DoesNotContain("sp_addextendedproperty", sql);
        Assert.DoesNotContain("ADD SENSITIVITY CLASSIFICATION", sql);
    }

    /// <summary>
    /// Classification SQL should not be generated for the non-classifying ADD COLUMN.
    /// </summary>
    [Fact]
    public void Generate_AddColumnWithoutClassification_ProducesNoClassificationSql() {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextNoClassification>()
            .UseSqlServer("Server=.;Database=Test;")
            .Options;

        using var context = new TestDbContextNoClassification(options);
        var model = context.Model;

        var addColumnOp = new AddColumnOperation {
            Name = "Name",
            Table = "SimpleEntities",
            Schema = "dbo",
            ClrType = typeof(string),
            ColumnType = "nvarchar(max)",
            IsNullable = false
        };

        // Act
        var sql = GenerateSql(addColumnOp, model);

        // Assert
        Assert.Contains("ALTER TABLE", sql);
        Assert.Contains("ADD [Name]", sql);
        Assert.DoesNotContain("sp_addextendedproperty", sql);
        Assert.DoesNotContain("ADD SENSITIVITY CLASSIFICATION", sql);
    }

    #region Test Models and DbContexts

    private class TestEntity {
        public int Id { get; set; }
        public string Email { get; set; } = "";
    }

    private class TestDbContext : DbContext {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<TestEntity>(b => {
                b.ToTable("TestEntities", "dbo");

                b.Property(e => e.Email)
                    .HasAnnotation("DataClassification:Label", "Confidential")
                    .HasAnnotation("DataClassification:InformationType", "Email Address")
                    .HasAnnotation("DataClassification:Rank", "High");
            });
        }
    }

    private class SimpleEntity {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class TestDbContextNoClassification : DbContext {
        public TestDbContextNoClassification(DbContextOptions<TestDbContextNoClassification> options) : base(options) { }

        public DbSet<SimpleEntity> SimpleEntities => Set<SimpleEntity>();
    }

    #endregion

    #region Helper Methods

    private string GenerateSql(IEnumerable<MigrationOperation> operations, IModel? model = null) {
        var options = new DbContextOptionsBuilder()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=Fake;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        using var context = new DbContext(options);
        var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;

        var dependencies = serviceProvider.GetService(typeof(MigrationsSqlGeneratorDependencies)) as MigrationsSqlGeneratorDependencies;
        var commandBatchPreparer = serviceProvider.GetService(typeof(ICommandBatchPreparer)) as ICommandBatchPreparer;

        if (dependencies == null || commandBatchPreparer == null)
            throw new InvalidOperationException("Could not resolve required services");

        var generator = new DataClassificationSqlGenerator(dependencies, commandBatchPreparer);

   
        var opsList = operations.ToList();
        var commands = generator.Generate(opsList, model);

        var sql = string.Join("\n", commands.Select(c => c.CommandText));
        return sql;
    }

    private string GenerateSql(MigrationOperation operation)
        => GenerateSql(new[] { operation }, null);

    private string GenerateSql(MigrationOperation operation, IModel model)
        => GenerateSql(new[] { operation }, model);

    #endregion

}
