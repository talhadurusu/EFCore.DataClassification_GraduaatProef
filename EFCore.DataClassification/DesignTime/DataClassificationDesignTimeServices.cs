using EFCore.DataClassification.Generators;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.DataClassification.DesignTime {
    public class DataClassificationDesignTimeServices : IDesignTimeServices {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection) {
            serviceCollection.AddSingleton<IMigrationsCodeGenerator, DataClassificationMigrationsGenerator>();
            serviceCollection.AddSingleton<ICSharpMigrationOperationGenerator, DataClassificationMigrationOperationGenerator>();
        }
    }
}
