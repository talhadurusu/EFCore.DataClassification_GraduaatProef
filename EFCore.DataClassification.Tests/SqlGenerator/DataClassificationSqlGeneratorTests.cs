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

    /// <summary>
    /// CRITICAL TEST: Tests that Generate(CreateTableOperation) calls base and processes columns
    /// This verifies the override exists and doesn't break the base functionality.
    /// 
    /// Note: Full integration testing of CREATE TABLE + classification requires
    /// a real migration scenario which is covered by integration tests.
    /// This unit test verifies the method override works without errors.
    /// </summary>
    [Fact]
    public void Generate_CreateTableOperation_CallsBaseWithoutError()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer("Server=.;Database=Test;")
            .Options;

        IModel model;
        using (var context = new TestDbContext(options))
        {
            model = context.Model;
        }

        var createTableOp = new CreateTableOperation
        {
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
            PrimaryKey = new AddPrimaryKeyOperation
            {
                Name = "PK_TestEntities",
                Table = "TestEntities",
                Schema = "dbo",
                Columns = new[] { "Id" }
            }
        };

        // Act
        var sql = GenerateSql(createTableOp, model);

        // Assert - Verify CREATE TABLE was generated (base functionality works)
        Assert.Contains("CREATE TABLE", sql);
        Assert.Contains("[TestEntities]", sql);
        Assert.Contains("[Id]", sql);
        Assert.Contains("[Email]", sql);
        Assert.Contains("sp_addextendedproperty", sql);
        Assert.Contains("ADD SENSITIVITY CLASSIFICATION", sql);


        // The method should execute without throwing exceptions
        // Full classification SQL generation is tested in integration tests
    }

    /// <summary>
    /// CRITICAL TEST: Tests that Generate(AddColumnOperation) calls base and processes the column
    /// This verifies the override exists and doesn't break the base functionality.
    /// 
    /// Note: Full integration testing of ADD COLUMN + classification requires
    /// a real migration scenario which is covered by integration tests.
    /// This unit test verifies the method override works without errors.
    /// </summary>
    [Fact]
    public void Generate_AddColumnOperation_CallsBaseWithoutError()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer("Server=.;Database=Test;")
            .Options;

        IModel model;
        using (var context = new TestDbContext(options))
        {
            model = context.Model;
        }

        var addColumnOp = new AddColumnOperation
        {
            Name = "Email",
            Table = "TestEntities",
            Schema = "dbo",
            ClrType = typeof(string),
            ColumnType = "nvarchar(max)",
            IsNullable = false
        };

        // Act
        var sql = GenerateSql(addColumnOp, model);

        // Assert - Verify ALTER TABLE ADD was generated (base functionality works)
        Assert.Contains("ALTER TABLE", sql);
        Assert.Contains("ADD [Email]", sql);
        Assert.Contains("sp_addextendedproperty", sql);
        Assert.Contains("ADD SENSITIVITY CLASSIFICATION", sql);


    }

    /// <summary>
    /// TEST: Verifies that CREATE TABLE without classified columns doesn't generate classification SQL
    /// </summary>
    [Fact]
    public void Generate_CreateTableWithoutClassification_ProducesNoClassificationSql()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextNoClassification>()
            .UseSqlServer("Server=.;Database=Test;")
            .Options;

        using var context = new TestDbContextNoClassification(options);
        var model = context.Model;

        var createTableOp = new CreateTableOperation
        {
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
    /// TEST: Verifies that ADD COLUMN without classification doesn't generate classification SQL
    /// </summary>
    [Fact]
    public void Generate_AddColumnWithoutClassification_ProducesNoClassificationSql()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextNoClassification>()
            .UseSqlServer("Server=.;Database=Test;")
            .Options;

        using var context = new TestDbContextNoClassification(options);
        var model = context.Model;

        var addColumnOp = new AddColumnOperation
        {
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

    private class TestEntity
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<TestEntity>(b =>
            {
                b.ToTable("TestEntities", "dbo"); 

                b.Property(e => e.Email)
                    .HasAnnotation("DataClassification:Label", "Confidential")
                    .HasAnnotation("DataClassification:InformationType", "Email Address")
                    .HasAnnotation("DataClassification:Rank", "High");
            });
        }
    }

    private class SimpleEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private class TestDbContextNoClassification : DbContext
    {
        public TestDbContextNoClassification(DbContextOptions<TestDbContextNoClassification> options) : base(options) { }
        public DbSet<SimpleEntity> SimpleEntities => Set<SimpleEntity>();
    }

    #endregion

    #region Helper Methods

    private string GenerateSql(MigrationOperation operation)
    {
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
        
        var commands = generator.Generate(new[] { operation }, null);
        var sql = string.Join("\n", commands.Select(c => c.CommandText));

        return sql;
    }

    private string GenerateSql(MigrationOperation operation, IModel model)
    {
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
        
        var commands = generator.Generate(new[] { operation }, model);
        var sql = string.Join("\n", commands.Select(c => c.CommandText));

        return sql;
    }

    #endregion
}
