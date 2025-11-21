using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using EFCore.DataClassification.Constants;

namespace EFCore.DataClassification.Infrastructure {
    public class DataClassificationSqlGenerator : SqlServerMigrationsSqlGenerator {
        public DataClassificationSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            ICommandBatchPreparer commandBatchPreparer)
            : base(dependencies, commandBatchPreparer) {
        }

        protected override void Generate(
            CreateTableOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true) {
            // önce normal CREATE TABLE
            base.Generate(operation, model, builder, terminate);

            // sonra kolonlar üzerinden dönüp SENSITIVITY ekle
            foreach (var column in operation.Columns) {
                ProcessColumn(column, operation.Schema, operation.Name, builder);
            }
        }

        private void ProcessColumn(
            ColumnOperation column,
            string? schema,
            string tableName,
            MigrationCommandListBuilder builder) {
            var rankAnnotation = column.FindAnnotation(DataClassificationConstants.Rank);

            if (rankAnnotation?.Value == null)
                return;

            var label = column.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString();
            var infoType = column.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString();
            var rank = rankAnnotation.Value.ToString() ?? string.Empty;

            var safeLabel = GenerateStringLiteral(label);
            var safeInfoType = GenerateStringLiteral(infoType);

            builder
                .Append("ADD SENSITIVITY CLASSIFICATION TO ")
                .Append(Dependencies.SqlGenerationHelper
                    .DelimitIdentifier(tableName, schema))
                .Append(".")
                .Append(Dependencies.SqlGenerationHelper
                    .DelimitIdentifier(column.Name))
                .AppendLine()
                .Append("WITH ( LABEL = ").Append(safeLabel)
                .Append(", INFORMATION_TYPE = ").Append(safeInfoType)
                .Append(", RANK = ").Append(rank.ToUpperInvariant())
                .AppendLine(" )")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        
        private string GenerateStringLiteral(string? value) {
            if (value == null) {
                return "NULL";
            }

            
            var mapping = Dependencies.TypeMappingSource.FindMapping(typeof(string))
                          ?? throw new InvalidOperationException("String type mapping not found.");

            return mapping.GenerateSqlLiteral(value);
        }
    }
}
