using EFCore.DataClassification.Generators;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;

namespace EFCore.DataClassification.WebApi {
    public class Design : IDesignTimeServices {
        public void ConfigureDesignTimeServices(IServiceCollection services) {
            services.AddSingleton<IMigrationsCodeGenerator, DataClassificationMigrationsGenerator>();
            services.AddSingleton<ICSharpMigrationOperationGenerator, DataClassificationMigrationOperationGenerator>();
        }
    }
}
