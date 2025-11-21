using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.DataClassification.Extensions {
    public static class DbContextOptionsBuilderExtensions {
        /// <summary>
        /// EF Core Data Classification eklentisini aktif eder.
        /// Kullanım: options.UseSqlServer(..).UseDataClassification();
        /// </summary>
        public static DbContextOptionsBuilder UseDataClassification(this DbContextOptionsBuilder optionsBuilder) {
            var extension = optionsBuilder.Options.FindExtension<DataClassificationDbContextOptionsExtension>()
                            ?? new DataClassificationDbContextOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}