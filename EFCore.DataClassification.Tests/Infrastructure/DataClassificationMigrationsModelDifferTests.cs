using EFCore.DataClassification.Extensions;
using EFCore.DataClassification.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace EFCore.DataClassification.Tests.Infrastructure;

/// <summary>
/// Tests for DataClassificationMigrationsModelDiffer
/// Verifies that the custom differ is properly registered in the DI container.
/// 
/// Note: Full diff behavior is tested in DataClassificationMigrationsModelDifferDiffTests.cs
/// </summary>
public class DataClassificationMigrationsModelDifferTests
{
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
}
