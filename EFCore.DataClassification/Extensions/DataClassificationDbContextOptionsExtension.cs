using System;
using System.Linq;
using EFCore.DataClassification.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.DataClassification.Extensions {
    public class DataClassificationDbContextOptionsExtension : IDbContextOptionsExtension {
        public void ApplyServices(IServiceCollection services) {
            services.AddScoped<IMigrationsSqlGenerator, DataClassificationSqlGenerator>();
            DecorateMigrationsModelDiffer(services);
        }

        private static void DecorateMigrationsModelDiffer(IServiceCollection services) {
            var descriptor = services.LastOrDefault(d => d.ServiceType == typeof(IMigrationsModelDiffer));
            if (descriptor is null) return;

            services.Remove(descriptor);

            services.Add(new ServiceDescriptor(
                typeof(IMigrationsModelDiffer),
                sp => {
                    var inner =
                        descriptor.ImplementationInstance as IMigrationsModelDiffer
                        ?? (descriptor.ImplementationFactory is not null
                            ? (IMigrationsModelDiffer)descriptor.ImplementationFactory(sp)
                            : (IMigrationsModelDiffer)ActivatorUtilities.CreateInstance(sp, descriptor.ImplementationType!));

                    return new MigrationsModelDifferDecorator(inner);
                },
                descriptor.Lifetime));
        }

        public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);
        public void Validate(IDbContextOptions options) { }

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo {
            public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }
            public override bool IsDatabaseProvider => false;
            public override string LogFragment => "DataClassification";
            public override int GetServiceProviderHashCode() => 0;
            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;
            public override void PopulateDebugInfo(System.Collections.Generic.IDictionary<string, string> debugInfo)
                => debugInfo["DataClassification"] = "1";
        }
    }
}
