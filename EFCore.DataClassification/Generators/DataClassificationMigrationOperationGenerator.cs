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
            builder
                .AppendLine(".Operations.Add(new CreateDataClassificationOperation")
                .AppendLine("{")
                .IncrementIndent()
                .AppendLine($"Schema = {Dependencies.CSharpHelper.Literal(op.Schema)},")
                .AppendLine($"Table = {Dependencies.CSharpHelper.Literal(op.Table)},")
                .AppendLine($"Column = {Dependencies.CSharpHelper.Literal(op.Column)},")
                .AppendLine($"Label = {Dependencies.CSharpHelper.Literal(op.Label)},")
                .AppendLine($"InformationType = {Dependencies.CSharpHelper.Literal(op.InformationType)},")
                .AppendLine($"Rank = {Dependencies.CSharpHelper.Literal(op.Rank)},")
                .DecrementIndent()
                .AppendLine("})");
        }

        private void GenerateRemove(RemoveDataClassificationOperation op, IndentedStringBuilder builder) {
            builder
                .AppendLine(".Operations.Add(new RemoveDataClassificationOperation")
                .AppendLine("{")
                .IncrementIndent()
                .AppendLine($"Schema = {Dependencies.CSharpHelper.Literal(op.Schema)},")
                .AppendLine($"Table = {Dependencies.CSharpHelper.Literal(op.Table)},")
                .AppendLine($"Column = {Dependencies.CSharpHelper.Literal(op.Column)}")
                .DecrementIndent()
                .AppendLine("})"); 
        }
    }
}