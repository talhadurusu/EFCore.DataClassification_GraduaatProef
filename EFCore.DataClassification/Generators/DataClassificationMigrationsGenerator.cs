using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.DataClassification.Generators {
    public class DataClassificationMigrationsGenerator : CSharpMigrationsGenerator {
        public DataClassificationMigrationsGenerator(
            MigrationsCodeGeneratorDependencies dependencies,
            CSharpMigrationsGeneratorDependencies csharpDependencies)
            : base(dependencies, csharpDependencies) {
        }

        protected override IEnumerable<string> GetNamespaces(IEnumerable<MigrationOperation> operations) =>
            base.GetNamespaces(operations)
                .Concat(new[] {
                    "EFCore.DataClassification.Operations",
                    "EFCore.DataClassification.Extensions"
                });
    }
}