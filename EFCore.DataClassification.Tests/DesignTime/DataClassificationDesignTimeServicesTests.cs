using EFCore.DataClassification.DesignTime;
using EFCore.DataClassification.Generators;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFCore.DataClassification.Tests.DesignTime;

public class DataClassificationDesignTimeServicesTests {
    [Fact]
    public void ConfigureDesignTimeServices_registers_generators() {
        // Arrange
        var services = new ServiceCollection();
        IDesignTimeServices dt = new DataClassificationDesignTimeServices();

        // Act
        dt.ConfigureDesignTimeServices(services);

        // Assert (non resolve only to avoid needing dependencies)
        Assert.Contains(services, d =>
            d.ServiceType == typeof(IMigrationsCodeGenerator) &&
            d.ImplementationType == typeof(DataClassificationMigrationsGenerator));

        Assert.Contains(services, d =>
            d.ServiceType == typeof(ICSharpMigrationOperationGenerator) &&
            d.ImplementationType == typeof(DataClassificationMigrationOperationGenerator));
    }
}
