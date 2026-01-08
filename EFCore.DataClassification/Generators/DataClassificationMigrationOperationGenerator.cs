using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.DataClassification.Generators {
    public sealed class DataClassificationMigrationOperationGenerator : CSharpMigrationOperationGenerator {
        public DataClassificationMigrationOperationGenerator(
            CSharpMigrationOperationGeneratorDependencies dependencies)
            : base(dependencies) {
        }

        protected override void Generate(MigrationOperation operation, IndentedStringBuilder builder) {
            switch (operation) {
                case CreateDataClassificationOperation create:
                    GenerateCreate(create, builder);
                    return;

                case RemoveDataClassificationOperation remove:
                    GenerateRemove(remove, builder);
                    return;
            }

            base.Generate(operation, builder);
        }
        private void GenerateCreate(CreateDataClassificationOperation op, IndentedStringBuilder builder) {
            builder.Append(".AddDataClassification(");

            builder.Append($"table: {Dependencies.CSharpHelper.Literal(op.Table)}, ");
            builder.Append($"column: {Dependencies.CSharpHelper.Literal(op.Column)}");

            if (op.Schema is not null)
                builder.Append($", schema: {Dependencies.CSharpHelper.Literal(op.Schema)}");
            if (op.Label is not null)
                builder.Append($", label: {Dependencies.CSharpHelper.Literal(op.Label)}");
            if (op.InformationType is not null)
                builder.Append($", informationType: {Dependencies.CSharpHelper.Literal(op.InformationType)}");
            if (op.Rank is not null)
                builder.Append($", rank: {Dependencies.CSharpHelper.Literal(op.Rank)}");

            
            builder.Append(")");
        }

        private void GenerateRemove(RemoveDataClassificationOperation op, IndentedStringBuilder builder) {
            builder.Append(".DropDataClassification(");

            builder.Append($"table: {Dependencies.CSharpHelper.Literal(op.Table)}, ");
            builder.Append($"column: {Dependencies.CSharpHelper.Literal(op.Column)}");

            if (op.Schema is not null)
                builder.Append($", schema: {Dependencies.CSharpHelper.Literal(op.Schema)}");

            builder.Append(")");
        }

    }
}
