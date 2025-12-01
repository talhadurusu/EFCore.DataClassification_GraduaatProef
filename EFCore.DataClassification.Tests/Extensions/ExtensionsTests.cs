using EFCore.DataClassification.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace EFCore.DataClassification.Tests.Extensions;

/// <summary>
/// Tests for DbContext extensions
/// </summary>
public class ExtensionsTests
{
    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    [Fact]
    public void UseDataClassificationSqlServer_RegistersServices()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer("Server=.;Database=Test;");

        // Act
        optionsBuilder.UseDataClassificationSqlServer();

        // Assert
        var options = optionsBuilder.Options;
        Assert.NotNull(options);
        
        // Verify extension was added
        var extension = options.FindExtension<DataClassificationDbContextOptionsExtension>();
        Assert.NotNull(extension);
    }

    [Fact]
    public void UseDataClassificationSqlServer_CanBeCalledMultipleTimes()
    {
        // Arrange
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer("Server=.;Database=Test;");

        // Act - Should not throw
        optionsBuilder.UseDataClassificationSqlServer();
        optionsBuilder.UseDataClassificationSqlServer();

        // Assert
        var options = optionsBuilder.Options;
        Assert.NotNull(options);
    }

    [Fact]
    public void DataClassificationExtension_InfoProperty_ReturnsCorrectValues()
    {
        // Arrange
        var extension = new DataClassificationDbContextOptionsExtension();

        // Act
        var info = extension.Info;

        // Assert
        Assert.NotNull(info);
        Assert.False(info.IsDatabaseProvider);
        Assert.Equal("DataClassification", info.LogFragment);
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<TestEntity> Entities => Set<TestEntity>();
    }
}

