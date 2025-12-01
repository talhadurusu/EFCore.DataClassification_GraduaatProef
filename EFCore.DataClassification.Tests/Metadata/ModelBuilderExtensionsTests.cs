using EFCore.DataClassification.Annotations;
using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace EFCore.DataClassification.Tests.Metadata;

public class ModelBuilderExtensionsTests
{
    #region Test Models
    
    private class UserWithAttribute
    {
        public int Id { get; set; }

        [DataClassification("Confidential", "Email Address", SensitivityRank.High)]
        public string Email { get; set; } = "";

        public string Name { get; set; } = "";
    }

    private class UserWithNoneRank
    {
        public int Id { get; set; }

        [DataClassification("Public", "Display Name", SensitivityRank.None)]
        public string DisplayName { get; set; } = "";
    }

    private class UserWithoutAttribute
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
    }

    #endregion

    [Fact]
    public void UseDataClassification_SetsAllAnnotations_WhenAttributePresent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextWithAttribute>()
            .UseInMemoryDatabase(databaseName: "test_attribute_present")
            .Options;

        using var context = new TestDbContextWithAttribute(options);

        // Assert
        var entity = context.Model.FindEntityType(typeof(UserWithAttribute))!;
        var emailProperty = entity.FindProperty(nameof(UserWithAttribute.Email))!;

        Assert.Equal("Confidential", emailProperty.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString());
        Assert.Equal("Email Address", emailProperty.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString());
        Assert.Equal("High", emailProperty.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString());
    }

    [Fact]
    public void UseDataClassification_DoesNotSetAnnotations_WhenAttributeAbsent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextWithoutAttribute>()
            .UseInMemoryDatabase(databaseName: "test_no_attribute")
            .Options;

        using var context = new TestDbContextWithoutAttribute(options);

        // Assert
        var entity = context.Model.FindEntityType(typeof(UserWithoutAttribute))!;
        var emailProperty = entity.FindProperty(nameof(UserWithoutAttribute.Email))!;

        Assert.Null(emailProperty.FindAnnotation(DataClassificationConstants.Label)?.Value);
        Assert.Null(emailProperty.FindAnnotation(DataClassificationConstants.InformationType)?.Value);
        Assert.Null(emailProperty.FindAnnotation(DataClassificationConstants.Rank)?.Value);
    }

    [Fact]
    public void UseDataClassification_HandlesNoneRank_WithoutException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextNoneRank>()
            .UseInMemoryDatabase(databaseName: "test_none_rank")
            .Options;

        using var context = new TestDbContextNoneRank(options);

        // Assert
        var entity = context.Model.FindEntityType(typeof(UserWithNoneRank))!;
        var property = entity.FindProperty(nameof(UserWithNoneRank.DisplayName))!;

        Assert.Equal("Public", property.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString());
        Assert.Equal("Display Name", property.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString());
        Assert.Equal("None", property.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString());
    }

    [Fact]
    public void UseDataClassification_HandlesMultipleEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextMultiple>()
            .UseInMemoryDatabase(databaseName: "test_multiple")
            .Options;

        using var context = new TestDbContextMultiple(options);

        // Assert
        var entityWithAttr = context.Model.FindEntityType(typeof(UserWithAttribute))!;
        var emailWithAttr = entityWithAttr.FindProperty(nameof(UserWithAttribute.Email))!;
        Assert.NotNull(emailWithAttr.FindAnnotation(DataClassificationConstants.Label)?.Value);

        var entityWithoutAttr = context.Model.FindEntityType(typeof(UserWithoutAttribute))!;
        var emailWithoutAttr = entityWithoutAttr.FindProperty(nameof(UserWithoutAttribute.Email))!;
        Assert.Null(emailWithoutAttr.FindAnnotation(DataClassificationConstants.Label)?.Value);
    }

    #region Test DbContexts

    private class TestDbContextWithAttribute : DbContext
    {
        public TestDbContextWithAttribute(DbContextOptions<TestDbContextWithAttribute> options) : base(options) { }
        public DbSet<UserWithAttribute> Users => Set<UserWithAttribute>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();
        }
    }

    private class TestDbContextWithoutAttribute : DbContext
    {
        public TestDbContextWithoutAttribute(DbContextOptions<TestDbContextWithoutAttribute> options) : base(options) { }
        public DbSet<UserWithoutAttribute> Users => Set<UserWithoutAttribute>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();
        }
    }

    private class TestDbContextNoneRank : DbContext
    {
        public TestDbContextNoneRank(DbContextOptions<TestDbContextNoneRank> options) : base(options) { }
        public DbSet<UserWithNoneRank> Users => Set<UserWithNoneRank>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();
        }
    }

    private class TestDbContextMultiple : DbContext
    {
        public TestDbContextMultiple(DbContextOptions<TestDbContextMultiple> options) : base(options) { }
        public DbSet<UserWithAttribute> UsersWithAttr => Set<UserWithAttribute>();
        public DbSet<UserWithoutAttribute> UsersWithoutAttr => Set<UserWithoutAttribute>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();
        }
    }

    #endregion
}
