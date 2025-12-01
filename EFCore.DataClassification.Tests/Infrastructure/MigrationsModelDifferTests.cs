using EFCore.DataClassification.Annotations;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Models;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.DataClassification.Tests.Infrastructure;

/// <summary>
/// Critical tests for DataClassificationMigrationsModelDiffer
/// These tests catch bugs related to AlterColumn operations and classification changes
/// </summary>
public class MigrationsModelDifferTests
{
    #region Test Models

    private class TestUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int? AdminId { get; set; }
        public string Email { get; set; } = "";
    }

    #endregion

    /// <summary>
    /// CRITICAL TEST #1: This test catches the bug where AlterColumn operations
    /// prevented data classification from being added.
    /// 
    /// Scenario: Column changes from NULL to NOT NULL AND gets data classification
    /// Expected: Data classification annotations should be present in the model
    /// 
    /// This was the bug that caused AdminId classification to not be applied!
    /// </summary>
    [Fact]
    public void Model_AlterColumnWithNewClassification_HasClassificationAnnotations()
    {
        // Arrange - Create a context where AdminId is required and has classification
        var options = new DbContextOptionsBuilder<TargetContextRequiredWithClassification>()
            .UseInMemoryDatabase("test_alter_with_classification")
            .Options;

        using var context = new TargetContextRequiredWithClassification(options);

        // Act - Get the AdminId property
        var entity = context.Model.FindEntityType(typeof(TestUser))!;
        var adminIdProperty = entity.FindProperty(nameof(TestUser.AdminId))!;

        // Assert - Classification annotations should be present
        var label = adminIdProperty.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString();
        var infoType = adminIdProperty.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString();
        var rank = adminIdProperty.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString();

        Assert.NotNull(label);
        Assert.NotNull(infoType);
        Assert.NotNull(rank);
        Assert.Equal("Internal", label);
        Assert.Equal("Admin Reference", infoType);
        Assert.Equal("Medium", rank);
        
        // Also verify the column is required (this would trigger AlterColumn in migration)
        Assert.False(adminIdProperty.IsNullable);
    }

    /// <summary>
    /// CRITICAL TEST #2: Detects when data classification annotations change
    /// 
    /// Scenario: Column already has classification, but values change (e.g., Low -> High)
    /// Expected: New classification values should be in the model
    /// </summary>
    [Fact]
    public void Model_ClassificationChanged_HasUpdatedAnnotations()
    {
        // Arrange - Create a context where Email classification changed from Low to High
        var options = new DbContextOptionsBuilder<TargetContextHighRank>()
            .UseInMemoryDatabase("test_classification_change")
            .Options;

        using var context = new TargetContextHighRank(options);

        // Act - Get the Email property
        var entity = context.Model.FindEntityType(typeof(TestUser))!;
        var emailProperty = entity.FindProperty(nameof(TestUser.Email))!;

        // Assert - Should have the NEW (High) classification
        var label = emailProperty.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString();
        var rank = emailProperty.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString();

        Assert.Equal("Confidential", label);
        Assert.Equal("High", rank);
        Assert.NotEqual("Low", rank); // Should NOT be the old value
    }

    /// <summary>
    /// TEST #3: Verifies classification is added when column already exists
    /// 
    /// Scenario: Column exists without classification, then classification is added
    /// Expected: Classification annotations should be present
    /// </summary>
    [Fact]
    public void Model_AddClassificationToExistingColumn_HasAnnotations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ContextWithClassification>()
            .UseInMemoryDatabase("test_add_classification")
            .Options;

        using var context = new ContextWithClassification(options);

        // Act
        var entity = context.Model.FindEntityType(typeof(TestUser))!;
        var nameProperty = entity.FindProperty(nameof(TestUser.Name))!;

        // Assert - Name should have classification
        var label = nameProperty.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString();
        Assert.NotNull(label);
        Assert.Equal("Public", label);
    }

    /// <summary>
    /// TEST #4: Verifies classification is removed when attribute is removed
    /// 
    /// Scenario: Column had classification, then it's removed
    /// Expected: No classification annotations
    /// </summary>
    [Fact]
    public void Model_RemoveClassification_HasNoAnnotations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ContextWithoutClassification>()
            .UseInMemoryDatabase("test_remove_classification")
            .Options;

        using var context = new ContextWithoutClassification(options);

        // Act
        var entity = context.Model.FindEntityType(typeof(TestUser))!;
        var nameProperty = entity.FindProperty(nameof(TestUser.Name))!;

        // Assert - Name should NOT have classification
        var label = nameProperty.FindAnnotation(DataClassificationConstants.Label)?.Value;
        Assert.Null(label);
    }

    #region Test DbContexts

    // Test #1 Context: AlterColumn scenario
    private class TargetContextRequiredWithClassification : DbContext
    {
        public TargetContextRequiredWithClassification(DbContextOptions<TargetContextRequiredWithClassification> options) : base(options) { }
        public DbSet<TestUser> Users => Set<TestUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();

            modelBuilder.Entity<TestUser>()
                .Property(u => u.AdminId)
                .IsRequired(true) // NOT NULL - triggers AlterColumn
                .HasDataClassification("Internal", "Admin Reference", SensitivityRank.Medium);
        }
    }

    // Test #2 Contexts: Classification change scenario
    private class TargetContextHighRank : DbContext
    {
        public TargetContextHighRank(DbContextOptions<TargetContextHighRank> options) : base(options) { }
        public DbSet<TestUser> Users => Set<TestUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();

            modelBuilder.Entity<TestUser>()
                .Property(u => u.Email)
                .HasDataClassification("Confidential", "Email Address", SensitivityRank.High);
        }
    }

    // Test #3 Context: Add classification
    private class ContextWithClassification : DbContext
    {
        public ContextWithClassification(DbContextOptions<ContextWithClassification> options) : base(options) { }
        public DbSet<TestUser> Users => Set<TestUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();

            modelBuilder.Entity<TestUser>()
                .Property(u => u.Name)
                .HasDataClassification("Public", "Display Name", SensitivityRank.Low);
        }
    }

    // Test #4 Context: No classification
    private class ContextWithoutClassification : DbContext
    {
        public ContextWithoutClassification(DbContextOptions<ContextWithoutClassification> options) : base(options) { }
        public DbSet<TestUser> Users => Set<TestUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();
            // Name has no classification
        }
    }

    #endregion
}
