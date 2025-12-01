using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Infrastructure;
using EFCore.DataClassification.Models;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Xunit;

namespace EFCore.DataClassification.Tests.Infrastructure;

/// <summary>
/// Tests for DataClassificationMigrationsModelDiffer
/// This class is CRITICAL as it detects data classification changes in the model
/// and generates the appropriate migration operations.
/// 
/// Note: These tests verify that the differ is registered and can process models.
/// Full integration testing requires real migration scenarios.
/// </summary>
public class DataClassificationMigrationsModelDifferTests
{
    #region Test Models

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    #endregion

    /// <summary>
    /// CRITICAL TEST: Verifies that DataClassificationMigrationsModelDiffer is registered
    /// and can be resolved from the service provider
    /// </summary>
    [Fact]
    public void DataClassificationMigrationsModelDiffer_IsRegistered()
    {
        // Arrange
        var options = new DbContextOptionsBuilder()
            .UseSqlServer("Server=.;Database=Test;")
            .UseDataClassificationSqlServer()
            .Options;

        using var context = new DbContext(options);
        var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;

        // Act
        var differ = serviceProvider.GetService(typeof(IMigrationsModelDiffer));

        // Assert
        Assert.NotNull(differ);
        Assert.IsType<DataClassificationMigrationsModelDiffer>(differ);
    }

    /// <summary>
    /// CRITICAL TEST: Verifies that the differ is of the correct type
    /// </summary>
    [Fact]
    public void DataClassificationMigrationsModelDiffer_IsCorrectType()
    {
        // Arrange
        var options = new DbContextOptionsBuilder()
            .UseSqlServer("Server=.;Database=Test;")
            .UseDataClassificationSqlServer()
            .Options;

        using var context = new DbContext(options);
        var serviceProvider = ((IInfrastructure<IServiceProvider>)context).Instance;

        // Act
        var differ = serviceProvider.GetService(typeof(IMigrationsModelDiffer));

        // Assert
        Assert.NotNull(differ);
        Assert.IsAssignableFrom<IMigrationsModelDiffer>(differ);
        Assert.IsType<DataClassificationMigrationsModelDiffer>(differ);
    }


    #region Test DbContexts

    private class TestContext1 : DbContext
    {
        public TestContext1(DbContextOptions<TestContext1> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.Ignore(e => e.Email);
            });
        }
    }

    private class TestContext2 : DbContext
    {
        public TestContext2(DbContextOptions<TestContext2> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.Ignore(e => e.Name);
            });
        }
    }

    private class TestContextWithClassification : DbContext
    {
        public TestContextWithClassification(DbContextOptions<TestContextWithClassification> options) : base(options) { }
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>(entity =>
            {
                entity.Property(e => e.Email)
                    .HasAnnotation("DataClassification:Label", "Confidential")
                    .HasAnnotation("DataClassification:InformationType", "Email Address")
                    .HasAnnotation("DataClassification:Rank", "High");
            });
        }
    }

    #endregion
}

