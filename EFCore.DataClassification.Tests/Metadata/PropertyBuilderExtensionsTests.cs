using EFCore.DataClassification.Annotations;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

namespace EFCore.DataClassification.Tests.Metadata;

public class PropertyBuilderExtensionsTests
{
    #region Test Models

    private class TestUser
    {
        public int Id { get; set; }
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
    }

    #endregion

    [Fact]
    public void HasDataClassification_Generic_SetsAllAnnotations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("test_generic")
            .Options;

        using var context = new TestDbContext(options);

        // Assert
        var entity = context.Model.FindEntityType(typeof(TestUser))!;
        var phoneProperty = entity.FindProperty(nameof(TestUser.Phone))!;

        Assert.Equal("Internal", phoneProperty.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString());
        Assert.Equal("Phone Number", phoneProperty.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString());
        Assert.Equal("Medium", phoneProperty.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString());
    }

    [Fact]
    public void HasDataClassification_NonGeneric_SetsAllAnnotations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextNonGeneric>()
            .UseInMemoryDatabase("test_nongeneric")
            .Options;

        using var context = new TestDbContextNonGeneric(options);

        // Assert
        var entity = context.Model.FindEntityType(typeof(TestUser))!;
        var emailProperty = entity.FindProperty(nameof(TestUser.Email))!;

        Assert.Equal("Confidential", emailProperty.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString());
        Assert.Equal("Email Address", emailProperty.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString());
        Assert.Equal("High", emailProperty.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString());
    }

    [Fact]
    public void HasDataClassification_WithNoneRank_SetsAnnotations()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextNone>()
            .UseInMemoryDatabase("test_none_rank")
            .Options;

        using var context = new TestDbContextNone(options);

        // Assert
        var entity = context.Model.FindEntityType(typeof(TestUser))!;
        var property = entity.FindProperty(nameof(TestUser.Phone))!;

        Assert.Equal("Public", property.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString());
        Assert.Equal("None", property.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString());
    }

    #region Test DbContexts

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestUser> Users => Set<TestUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestUser>()
                .Property(u => u.Phone)
                .HasDataClassification("Internal", "Phone Number", SensitivityRank.Medium);
        }
    }

    private class TestDbContextNonGeneric : DbContext
    {
        public TestDbContextNonGeneric(DbContextOptions<TestDbContextNonGeneric> options) : base(options) { }
        public DbSet<TestUser> Users => Set<TestUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestUser>()
                .Property("Email")
                .HasDataClassification("Confidential", "Email Address", SensitivityRank.High);
        }
    }

    private class TestDbContextNone : DbContext
    {
        public TestDbContextNone(DbContextOptions<TestDbContextNone> options) : base(options) { }
        public DbSet<TestUser> Users => Set<TestUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestUser>()
                .Property(u => u.Phone)
                .HasDataClassification("Public", "Public Info", SensitivityRank.None);
        }
    }

    #endregion
}

