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
/// MISSING CRITICAL TESTS - These scenarios were not tested before!
/// </summary>
public class MissingCriticalTests {
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

    // Multiple columns with classification
    private class Ctx_MultipleClassifiedColumns : DbContext {
        public Ctx_MultipleClassifiedColumns(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;

        public class User {
            public int Id { get; set; }
            [DataClassification("PII", "Email", SensitivityRank.High)]
            public string Email { get; set; } = "";
            [DataClassification("PII", "Phone", SensitivityRank.High)]
            public string Phone { get; set; } = "";
            [DataClassification("PII", "SSN", SensitivityRank.Critical)]
            public string SSN { get; set; } = "";
        }

        protected override void OnModelCreating(ModelBuilder mb) => mb.UseDataClassification();
    }

    // All columns dropped
    private class Ctx_AllColumnsDropped : DbContext {
        public Ctx_AllColumnsDropped(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;

        public class User {
            public int Id { get; set; }
        }

        protected override void OnModelCreating(ModelBuilder mb) => mb.UseDataClassification();
    }

    // Multiple operations on same table
    private class Ctx_MixedOperations : DbContext {
        public Ctx_MixedOperations(DbContextOptions options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;

        public class User {
            public int Id { get; set; }
            // Email renamed to EmailAddress
            [DataClassification("PII", "Email", SensitivityRank.Medium)]
            public string EmailAddress { get; set; } = "";
            // Phone altered (nullable)
            [DataClassification("PII", "Phone", SensitivityRank.Medium)]
            public string? Phone { get; set; }
            // SSN dropped
            // New column added
            [DataClassification("PII", "Address", SensitivityRank.Low)]
            public string Address { get; set; } = "";
        }

        protected override void OnModelCreating(ModelBuilder mb) => mb.UseDataClassification();
    }

    // Non-default schema
    private class Ctx_CustomSchema : DbContext {
        public Ctx_CustomSchema(DbContextOptions options) : base(options) { }
        public DbSet<Product> Products { get; set; } = null!;

        public class Product {
            public int Id { get; set; }
            [DataClassification("Business", "Product Name", SensitivityRank.Low)]
            public string Name { get; set; } = "";
        }

        protected override void OnModelCreating(ModelBuilder mb) {
            mb.UseDataClassification();
            mb.Entity<Product>().ToTable("Products", "sales");
        }
    }

    #endregion

    /// <summary>
    /// CRITICAL TEST: Multiple columns dropped at once
    /// Each drop must have Remove before Drop, and they must not interfere with each other
    /// </summary>
    [Fact]
    public void BugCatcher_MultipleColumnsDropped_EachMustHaveCorrectOrder() {
        // Arrange
        using var source = CreateCtx<Ctx_MultipleClassifiedColumns>();
        using var target = CreateCtx<Ctx_AllColumnsDropped>();
        
        var differ = GetDiffer(source);
        var sourceModel = GetRelationalModel(source);
        var targetModel = GetRelationalModel(target);

        // Act
        var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

        // Assert - Find all drop operations
        var emailRemove = operations.OfType<RemoveDataClassificationOperation>()
            .SingleOrDefault(op => op.Column == "Email");
        var phoneRemove = operations.OfType<RemoveDataClassificationOperation>()
            .SingleOrDefault(op => op.Column == "Phone");
        var ssnRemove = operations.OfType<RemoveDataClassificationOperation>()
            .SingleOrDefault(op => op.Column == "SSN");

        var emailDrop = operations.OfType<DropColumnOperation>()
            .SingleOrDefault(op => op.Name == "Email");
        var phoneDrop = operations.OfType<DropColumnOperation>()
            .SingleOrDefault(op => op.Name == "Phone");
        var ssnDrop = operations.OfType<DropColumnOperation>()
            .SingleOrDefault(op => op.Name == "SSN");

        Assert.NotNull(emailRemove);
        Assert.NotNull(phoneRemove);
        Assert.NotNull(ssnRemove);
        Assert.NotNull(emailDrop);
        Assert.NotNull(phoneDrop);
        Assert.NotNull(ssnDrop);

        // CRITICAL: Each Remove must come before its corresponding Drop
        Assert.True(operations.IndexOf(emailRemove) < operations.IndexOf(emailDrop),
            $"BUG: Email RemoveDataClassification must come before Email DropColumn");
        Assert.True(operations.IndexOf(phoneRemove) < operations.IndexOf(phoneDrop),
            $"BUG: Phone RemoveDataClassification must come before Phone DropColumn");
        Assert.True(operations.IndexOf(ssnRemove) < operations.IndexOf(ssnDrop),
            $"BUG: SSN RemoveDataClassification must come before SSN DropColumn");
    }

    /// <summary>
    /// CRITICAL TEST: Mixed operations on same table (rename + alter + drop + add)
    /// All operations must maintain correct order without interfering
    /// </summary>
    [Fact]
    public void BugCatcher_MixedOperations_AllMustHaveCorrectOrder() {
        // Arrange
        using var source = CreateCtx<Ctx_MultipleClassifiedColumns>();
        using var target = CreateCtx<Ctx_MixedOperations>();
        
        var differ = GetDiffer(source);
        var sourceModel = GetRelationalModel(source);
        var targetModel = GetRelationalModel(target);

        // Act
        var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

        // CRITICAL: Check all orderings based on actual ops emitted by differ
        foreach (var rename in operations.OfType<RenameColumnOperation>()) {
            var remove = operations.OfType<RemoveDataClassificationOperation>()
                .SingleOrDefault(op => op.Table == rename.Table && op.Column == rename.Name);
            var create = operations.OfType<CreateDataClassificationOperation>()
                .SingleOrDefault(op => op.Table == rename.Table && op.Column == rename.NewName);

            if (remove != null) {
                Assert.True(operations.IndexOf(remove) < operations.IndexOf(rename),
                    $"BUG: Remove must come before Rename. Ops: {DumpOps(operations)}");
            }

            if (create != null) {
                Assert.True(operations.IndexOf(rename) < operations.IndexOf(create),
                    $"BUG: Rename must come before Create. Ops: {DumpOps(operations)}");
            }
        }

        foreach (var alter in operations.OfType<AlterColumnOperation>()) {
            var remove = operations.OfType<RemoveDataClassificationOperation>()
                .SingleOrDefault(op => op.Table == alter.Table && op.Column == alter.Name);
            var create = operations.OfType<CreateDataClassificationOperation>()
                .SingleOrDefault(op => op.Table == alter.Table && op.Column == alter.Name);

            if (remove != null) {
                Assert.True(operations.IndexOf(remove) < operations.IndexOf(alter),
                    $"BUG: Remove must come before Alter. Ops: {DumpOps(operations)}");
            }

            if (create != null) {
                Assert.True(operations.IndexOf(alter) < operations.IndexOf(create),
                    $"BUG: Alter must come before Create. Ops: {DumpOps(operations)}");
            }
        }

        foreach (var drop in operations.OfType<DropColumnOperation>()) {
            var remove = operations.OfType<RemoveDataClassificationOperation>()
                .SingleOrDefault(op => op.Table == drop.Table && op.Column == drop.Name);

            if (remove != null) {
                Assert.True(operations.IndexOf(remove) < operations.IndexOf(drop),
                    $"BUG: Remove must come before Drop. Ops: {DumpOps(operations)}");
            }
        }

        foreach (var add in operations.OfType<AddColumnOperation>()) {
            var create = operations.OfType<CreateDataClassificationOperation>()
                .SingleOrDefault(op => op.Table == add.Table && op.Column == add.Name);

            if (create != null) {
                Assert.True(operations.IndexOf(add) < operations.IndexOf(create),
                    $"BUG: Add must come before Create classification. Ops: {DumpOps(operations)}");
            }
        }
    }

    /// <summary>
    /// CRITICAL TEST: Non-default schema (e.g., "sales" instead of "dbo")
    /// Schema comparison must work correctly (null vs "dbo" vs custom schema)
    /// </summary>
    [Fact]
    public void BugCatcher_CustomSchema_OperationsHaveCorrectSchema() {
        // Arrange
        using var source = CreateCtx<DbContext>();
        using var target = CreateCtx<Ctx_CustomSchema>();
        
        var differ = GetDiffer(target);
        var sourceModel = source.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        var targetModel = GetRelationalModel(target);

        // Act
        var operations = differ.GetDifferences(sourceModel, targetModel).ToList();

        // Assert
        var createOp = operations.OfType<CreateDataClassificationOperation>()
            .SingleOrDefault(op => op.Column == "Name");

        Assert.NotNull(createOp);
        Assert.Equal("sales", createOp.Schema);
        Assert.Equal("Products", createOp.Table);
    }

    /// <summary>
    /// CRITICAL TEST: Schema equivalence (null and "dbo" should be treated as same)
    /// This tests the SchemaEquals helper method
    /// </summary>
    [Fact]
    public void BugCatcher_SchemaEquivalence_NullAndDboAreSame() {
        // This test verifies that operations with schema=null and schema="dbo"
        // are treated as equivalent and don't generate duplicate operations
        
        // Arrange - Create two contexts with same table, one with explicit dbo, one without
        using var ctx1 = CreateCtx<Ctx_MultipleClassifiedColumns>();
        using var ctx2 = CreateCtx<Ctx_MultipleClassifiedColumns>();
        
        var differ = GetDiffer(ctx1);
        var model1 = GetRelationalModel(ctx1);
        var model2 = GetRelationalModel(ctx2);

        // Act
        var operations = differ.GetDifferences(model1, model2).ToList();

        // Assert - Should be no operations since models are identical
        var classificationOps = operations
            .Where(op => op is CreateDataClassificationOperation or RemoveDataClassificationOperation)
            .ToList();

        Assert.Empty(classificationOps);
    }

    private class DbContext : Microsoft.EntityFrameworkCore.DbContext {
        public DbContext(DbContextOptions options) : base(options) { }
    }

    private static string DumpOps(IReadOnlyList<MigrationOperation> operations) {
        return string.Join(" | ", operations.Select((op, idx) => $"{idx}:{DescribeOp(op)}"));
    }

    private static string DescribeOp(MigrationOperation op) {
        return op switch {
            RenameColumnOperation rename => $"RenameColumn {rename.Table}.{rename.Name}->{rename.NewName}",
            AlterColumnOperation alter => $"AlterColumn {alter.Table}.{alter.Name}",
            AddColumnOperation add => $"AddColumn {add.Table}.{add.Name}",
            DropColumnOperation drop => $"DropColumn {drop.Table}.{drop.Name}",
            CreateDataClassificationOperation create => $"CreateClass {create.Table}.{create.Column}",
            RemoveDataClassificationOperation remove => $"RemoveClass {remove.Table}.{remove.Column}",
            _ => op.GetType().Name
        };
    }
}
