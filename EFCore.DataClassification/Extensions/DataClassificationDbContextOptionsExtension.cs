using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using EFCore.DataClassification.Infrastructure;
using System.Collections.Generic;

namespace EFCore.DataClassification.Extensions {
    public class DataClassificationDbContextOptionsExtension : IDbContextOptionsExtension {
        public void ApplyServices(IServiceCollection services) {
    
            services.AddScoped<IMigrationsSqlGenerator, DataClassificationSqlGenerator>();
           
            services.AddScoped<IMigrationsModelDiffer, DataClassificationMigrationsModelDiffer>();
        
            
        }

        public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

        public void Validate(IDbContextOptions options) { }

        private class ExtensionInfo : DbContextOptionsExtensionInfo {
            public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }
            public override bool IsDatabaseProvider => false;
            public override string LogFragment => "DataClassification";
            public override int GetServiceProviderHashCode() => 0;
            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;
            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) {
                debugInfo["DataClassification"] = "1";
            }
        }
    }
}