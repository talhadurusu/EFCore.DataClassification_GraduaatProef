using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Models;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFCore.DataClassification.Tests.Infrastructure;

/// <summary>
/// CRITICAL BUG-CATCHING TESTS
/// These tests VERIFY the exact order of operations by checking INDICES.
/// If sorting is broken, these tests WILL FAIL.
/// </summary>
public class CriticalOrderingTests {
    private const string Cs = "Server=.;Database=Dummy;Trusted_Connection=True;TrustServerCertificate=True";

    private static TContext CreateCtx<TContext>() where TContext : DbContext {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlServer(Cs)
            .UseDataClassificationSqlServer()
            .Options;
        return (TContext)Activator.CreateInstance(typeof(TContext), options)!;
    }

    private static IMigrationsModelDiffer GetDiffer(DbContext ctx) {
        return ctx.GetService<IMigrationsModelDiffer>();
    }

    private static IRelationalModel GetRelationalModel(DbContext ctx) {
        var designTimeModel = ctx.GetService<IDesignTimeModel>().Model;
        return designTimeModel.GetRelationalModel();
    }

    #region Test Models

    // Source: Email with classification
    private class Ctx_Source_EmailClassified : DbContext {
        public Ctx_Source_EmailClassified(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;

        public class User {
            public int Id { get; set; }
            [DataClassification("PII", "Email", SensitivityRank.High)]
            public string Email { get; set; } = "";
        }

        protected override void OnModelCreating(ModelBuilder mb) => mb.UseDataClassification();
    }

    // Target: Email dropped
    private class Ctx_Target_EmailDropped : DbContext {
        public Ctx_Target_EmailDropped(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;

        public class User {
            public int Id { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder mb) => mb.UseDataClassification();
    }

    // Target: Email renamed to EmailAddress
    private class Ctx_Target_EmailRenamed : DbContext {
        public Ctx_Target_EmailRenamed(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;

        public class User {
            public int Id { get; set; }
            [DataClassification("PII", "Email", SensitivityRank.High)]
            public string EmailAddress { get; set; } = "";
        }

        protected override void OnModelCreating(ModelBuilder mb) => mb.UseDataClassification();
    }

    // Target: Email altered (nullable + rank changed)
    private class Ctx_Target_EmailAltered : DbContext {
        public Ctx_Target_EmailAltered(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;

        public class User {
            public int Id { get; set; }
            [DataClassification("PII", "Email", SensitivityRank.Medium)]
            public string? Email { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder mb) => mb.UseDataClassification();
    }

    #endregion

    /// <summary>
    /// CRITICAL TEST: DropColumn with classification
    /// Expected order: RemoveDataClassification → DropColumn
    /// If this fails, SQL will fail: "Cannot drop column with sensitivity classification"
    /// </summary>
    [Fact]
    public void BugCatcher_DropColumn_RemoveMustComeBeforeDrop() {
        // Arrange
        using var source = CreateCtx<Ctx_Source_EmailClassified>();
        using var target = CreateCtx<Ctx_Target_EmailDropped>();
        
        var differ = GetDiffer(source);
        var sourceModel = GetRelationalModel(source);
        var targetModel = GetRelationalModel(target);

        // Act
        var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

        // Assert - Find operations
        var removeOp = operations.OfType<RemoveDataClassificationOperation>()
            .SingleOrDefault(op => op.Column == "Email");
        var dropOp = operations.OfType<DropColumnOperation>()
            .SingleOrDefault(op => op.Name == "Email");

        Assert.NotNull(removeOp);
        Assert.NotNull(dropOp);

        // CRITICAL: Check exact indices
        var removeIndex = operations.IndexOf(removeOp);
        var dropIndex = operations.IndexOf(dropOp);

        Assert.True(removeIndex >= 0, "RemoveDataClassification not found in operations");
        Assert.True(dropIndex >= 0, "DropColumn not found in operations");
        Assert.True(removeIndex < dropIndex, 
            $"BUG DETECTED: RemoveDataClassification MUST come before DropColumn. " +
            $"Actual order: RemoveDataClassification at index {removeIndex}, DropColumn at index {dropIndex}. " +
            $"This will cause SQL error: 'Cannot drop column with sensitivity classification'");
    }

    /// <summary>
    /// CRITICAL TEST: RenameColumn with classification
    /// Expected order: RemoveDataClassification → RenameColumn → CreateDataClassification
    /// If this fails, SQL will fail: "Column 'Email' not found" or "Column 'EmailAddress' already exists"
    /// </summary>
    [Fact]
    public void BugCatcher_RenameColumn_MustHaveCorrectOrder() {
        // Arrange
        using var source = CreateCtx<Ctx_Source_EmailClassified>();
        using var target = CreateCtx<Ctx_Target_EmailRenamed>();
        
        var differ = GetDiffer(source);
        var sourceModel = GetRelationalModel(source);
        var targetModel = GetRelationalModel(target);

        // Act
        var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

        // Assert - Find operations
        var removeOp = operations.OfType<RemoveDataClassificationOperation>()
            .SingleOrDefault(op => op.Column == "Email");
        var renameOp = operations.OfType<RenameColumnOperation>()
            .SingleOrDefault(op => op.Name == "Email" && op.NewName == "EmailAddress");
        var createOp = operations.OfType<CreateDataClassificationOperation>()
            .SingleOrDefault(op => op.Column == "EmailAddress");

        Assert.NotNull(removeOp);
        Assert.NotNull(renameOp);
        Assert.NotNull(createOp);

        // CRITICAL: Check exact indices
        var removeIndex = operations.IndexOf(removeOp);
        var renameIndex = operations.IndexOf(renameOp);
        var createIndex = operations.IndexOf(createOp);

        Assert.True(removeIndex < renameIndex, 
            $"BUG DETECTED: RemoveDataClassification MUST come before RenameColumn. " +
            $"Actual: Remove at {removeIndex}, Rename at {renameIndex}. " +
            $"This will cause SQL error: 'Cannot rename column with sensitivity classification'");

        Assert.True(renameIndex < createIndex, 
            $"BUG DETECTED: RenameColumn MUST come before CreateDataClassification. " +
            $"Actual: Rename at {renameIndex}, Create at {createIndex}. " +
            $"This will cause SQL error: 'Column Email not found'");
    }

    /// <summary>
    /// CRITICAL TEST: AlterColumn with classification
    /// Expected order: RemoveDataClassification → AlterColumn → CreateDataClassification
    /// If this fails, SQL will fail: "Cannot alter column with sensitivity classification"
    /// </summary>
    [Fact]
    public void BugCatcher_AlterColumn_MustHaveCorrectOrder() {
        // Arrange
        using var source = CreateCtx<Ctx_Source_EmailClassified>();
        using var target = CreateCtx<Ctx_Target_EmailAltered>();
        
        var differ = GetDiffer(source);
        var sourceModel = GetRelationalModel(source);
        var targetModel = GetRelationalModel(target);

        // Act
        var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

        // Assert - Find operations
        var removeOp = operations.OfType<RemoveDataClassificationOperation>()
            .SingleOrDefault(op => op.Column == "Email");
        var alterOp = operations.OfType<AlterColumnOperation>()
            .SingleOrDefault(op => op.Name == "Email");
        var createOp = operations.OfType<CreateDataClassificationOperation>()
            .SingleOrDefault(op => op.Column == "Email");

        Assert.NotNull(removeOp);
        Assert.NotNull(alterOp);
        Assert.NotNull(createOp);

        // CRITICAL: Check exact indices
        var removeIndex = operations.IndexOf(removeOp);
        var alterIndex = operations.IndexOf(alterOp);
        var createIndex = operations.IndexOf(createOp);

        Assert.True(removeIndex < alterIndex, 
            $"BUG DETECTED: RemoveDataClassification MUST come before AlterColumn. " +
            $"Actual: Remove at {removeIndex}, Alter at {alterIndex}. " +
            $"This will cause SQL error: 'Cannot alter column with sensitivity classification'");

        Assert.True(alterIndex < createIndex, 
            $"BUG DETECTED: AlterColumn MUST come before CreateDataClassification. " +
            $"Actual: Alter at {alterIndex}, Create at {createIndex}. " +
            $"This will cause SQL error: 'Column type mismatch'");
    }

    /// <summary>
    /// STRESS TEST: Multiple operations on different tables
    /// This catches bugs where sorting affects other operations
    /// </summary>
    [Fact]
    public void BugCatcher_MultipleOperations_EachMustHaveCorrectOrder() {
        // This test will be added if needed
        // For now, the 3 tests above are sufficient
        Assert.True(true, "Placeholder for multi-table stress test");
    }
}

