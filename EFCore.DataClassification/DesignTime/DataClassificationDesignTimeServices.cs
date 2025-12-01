using EFCore.DataClassification.Generators;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.DataClassification.DesignTime {
    public class DataClassificationDesignTimeServices : IDesignTimeServices {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection) {
            //Debugger.Launch();
            serviceCollection.AddSingleton<IMigrationsCodeGenerator, DataClassificationMigrationsGenerator>();
            serviceCollection.AddSingleton<ICSharpMigrationOperationGenerator, DataClassificationMigrationOperationGenerator>();
        }
    }
}
