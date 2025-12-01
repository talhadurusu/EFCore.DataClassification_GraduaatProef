using EFCore.DataClassification.Annotations;
using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EFCore.DataClassification.Tests.Integration;

/// <summary>
/// Integration tests that verify attribute and fluent API work together correctly
/// </summary>
public class IntegrationTests
{
    private class Product
    {
        public int Id { get; set; }

        [DataClassification("Internal", "Product Name", SensitivityRank.Low)]
        public string Name { get; set; } = "";

        public decimal Price { get; set; }
    }

    [Fact]
    public void AttributeAndFluentAPI_BothSetAnnotations_WithoutConflict()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase("test_integration")
            .Options;

        using var context = new TestDbContext(options);

        // Assert - Attribute on Name
        var productEntity = context.Model.FindEntityType(typeof(Product))!;
        var nameProperty = productEntity.FindProperty(nameof(Product.Name))!;
        
        Assert.Equal("Internal", nameProperty.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString());
        Assert.Equal("Product Name", nameProperty.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString());
        Assert.Equal("Low", nameProperty.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString());

        // Assert - Fluent API on Price
        var priceProperty = productEntity.FindProperty(nameof(Product.Price))!;
        Assert.Equal("Confidential", priceProperty.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString());
        Assert.Equal("Pricing", priceProperty.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString());
        Assert.Equal("Critical", priceProperty.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString());
    }

    [Fact]
    public void FluentAPI_OverridesAttribute_WhenBothPresent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContextOverride>()
            .UseInMemoryDatabase("test_override")
            .Options;

        using var context = new TestDbContextOverride(options);

        // Assert - Fluent API should override attribute
        var productEntity = context.Model.FindEntityType(typeof(Product))!;
        var nameProperty = productEntity.FindProperty(nameof(Product.Name))!;
        
        // Fluent API was applied after UseDataClassification, so it should override
        Assert.Equal("Public", nameProperty.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString());
        Assert.Equal("Display Name", nameProperty.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString());
        Assert.Equal("None", nameProperty.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString());
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasDataClassification("Confidential", "Pricing", SensitivityRank.Critical);
        }
    }

    private class TestDbContextOverride : DbContext
    {
        public TestDbContextOverride(DbContextOptions<TestDbContextOverride> options) : base(options) { }
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseDataClassification();

            // Override the attribute
            modelBuilder.Entity<Product>()
                .Property(p => p.Name)
                .HasDataClassification("Public", "Display Name", SensitivityRank.None);
        }
    }
}


