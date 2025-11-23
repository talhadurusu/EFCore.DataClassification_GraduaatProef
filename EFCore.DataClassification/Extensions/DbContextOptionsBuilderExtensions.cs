using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.DataClassification.Extensions {
    public static class DbContextOptionsBuilderExtensions {

      
        public static DbContextOptionsBuilder UseDataClassification(this DbContextOptionsBuilder optionsBuilder) {
            var extension = optionsBuilder.Options.FindExtension<DataClassificationDbContextOptionsExtension>()
                            ?? new DataClassificationDbContextOptionsExtension();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            return optionsBuilder;
        }
    }
}