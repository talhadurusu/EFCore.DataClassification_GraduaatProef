using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using EFCore.DataClassification.Infrastructure;
using System.Collections.Generic;

namespace EFCore.DataClassification {

    // Bu sınıf, EF Core'a bizim yazdığımız eklentiyi tanıtan "kimlik kartı"dır.
    public class DataClassificationDbContextOptionsExtension : IDbContextOptionsExtension {
        public void ApplyServices(IServiceCollection services) {
            // İŞTE SİHİRLİ SATIR:
            // Standart IMigrationsSqlGenerator servisini, bizim yazdığımız DataClassificationSqlGenerator ile değiştiriyoruz.
            services.AddScoped<IMigrationsSqlGenerator, DataClassificationSqlGenerator>();
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
