using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.DataClassification.Extensions {
    public static class DbContextOptionsBuilderExtensions {

        /// <summary>
        /// Adds DataClassification services tailored for SQL Server.
        /// Replaces the default IMigrationsSqlGenerator.
        /// </summary>
       

        public static DbContextOptionsBuilder UseDataClassificationSqlServer(this DbContextOptionsBuilder optionsBuilder) {
            // Extension zaten ApplyServices içinde servisi değiştiriyor.
            // Tek yapmamız gereken bu extension'ı options'a eklemek.
            var extension = optionsBuilder.Options.FindExtension<DataClassificationDbContextOptionsExtension>()
                            ?? new DataClassificationDbContextOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}
