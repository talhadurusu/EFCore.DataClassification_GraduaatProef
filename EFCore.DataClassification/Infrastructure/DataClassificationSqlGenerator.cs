using System.Linq;
using EFCore.DataClassification.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace EFCore.DataClassification.Infrastructure {
  
    /// DataClassification annotation'larına göre
    /// hem kendi extended property'lerimizi,
    /// hem de SQL Server'ın resmi "sensitivity classification"
    /// metadata'sını (ADD SENSITIVITY CLASSIFICATION) üreten generator.
  
    public sealed class DataClassificationSqlGenerator : SqlServerMigrationsSqlGenerator {
        public DataClassificationSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            ICommandBatchPreparer commandBatchPreparer)
            : base(dependencies, commandBatchPreparer) {
        }

        // TABLO OLUŞTURULURKEN
        protected override void Generate(
            CreateTableOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true) {
            // Önce normal CREATE TABLE
            base.Generate(operation, model, builder, terminate);

            if (model is null)
                return;

            var relationalModel = model.GetRelationalModel();
            var table = relationalModel.FindTable(operation.Name, operation.Schema);
            if (table is null)
                return;

            foreach (var columnOp in operation.Columns) {
                var column = table.FindColumn(columnOp.Name);
                if (column is null)
                    continue;

                var property = column.PropertyMappings.FirstOrDefault()?.Property;
                if (property is null)
                    continue;

                WriteDataClassification(
                    builder,
                    operation.Schema,
                    operation.Name,
                    columnOp.Name,
                    property);
            }
        }

        // KOLON EKLENİRKEN
        protected override void Generate(
            AddColumnOperation operation,
            IModel? model,
            MigrationCommandListBuilder builder,
            bool terminate = true) {

            // Önce normal ALTER TABLE ADD
            base.Generate(operation, model, builder, terminate);

            if (model is null)
                return;

            var relationalModel = model.GetRelationalModel();
            var table = relationalModel.FindTable(operation.Table, operation.Schema);
            if (table is null)
                return;

            var column = table.FindColumn(operation.Name);
            if (column is null)
                return;

            var property = column.PropertyMappings.FirstOrDefault()?.Property;
            if (property is null)
                return;

            WriteDataClassification(
                builder,
                operation.Schema,
                operation.Table,
                operation.Name,
                property);
        }

        private void WriteDataClassification(
            MigrationCommandListBuilder builder,
            string? schema,
            string tableName,
            string columnName,
            IProperty property) {
            var schemaName = string.IsNullOrEmpty(schema) ? "dbo" : schema;

            // Annotation'lardan değerleri oku
            var label = property.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString();
            var infoType = property.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString();
            var rank = property.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString();

            // 1) Senin kendi extended property'lerin (DataClassification:*)
            if (!string.IsNullOrWhiteSpace(label)) {
                AppendExtendedProperty(
                    builder, schemaName, tableName, columnName,
                    DataClassificationConstants.Label, label);
            }

            if (!string.IsNullOrWhiteSpace(infoType)) {
                AppendExtendedProperty(
                    builder, schemaName, tableName, columnName,
                    DataClassificationConstants.InformationType, infoType);
            }

            if (!string.IsNullOrWhiteSpace(rank)) {
                AppendExtendedProperty(
                    builder, schemaName, tableName, columnName,
                    DataClassificationConstants.Rank, rank);
            }

            // 2) SQL Server 2019+ resmi metadata
            AppendSensitivityClassification(
                builder, schemaName, tableName, columnName,
                label, infoType, rank);
        }

        private void AppendExtendedProperty(
            MigrationCommandListBuilder builder,
            string schemaName,
            string tableName,
            string columnName,
            string propertyName,
            string propertyValue) {
            builder
                .AppendLine(
                    $"""
                     EXEC sys.sp_addextendedproperty
                         @name = N'{propertyName}',
                         @value = N'{propertyValue}',
                         @level0type = N'SCHEMA', @level0name = N'{schemaName}',
                         @level1type = N'TABLE',  @level1name = N'{tableName}',
                         @level2type = N'COLUMN', @level2name = N'{columnName}';
                     """)
                .EndCommand();
        }

        private void AppendSensitivityClassification(
            MigrationCommandListBuilder builder,
            string schemaName,
            string tableName,
            string columnName,
            string? label,
            string? informationType,
            string? rank) {
            // Hiç veri yoksa boşuna SQL üretme
            if (string.IsNullOrWhiteSpace(label)
                && string.IsNullOrWhiteSpace(informationType)
                && string.IsNullOrWhiteSpace(rank)) {
                return;
            }

            // Rank -> SQL'in beklediği formata çevir (LOW, MEDIUM, HIGH, CRITICAL...)
            string? sqlRank = null;
            if (!string.IsNullOrWhiteSpace(rank)) {
                sqlRank = rank.ToUpperInvariant(); // SensitivityRank.High -> "HIGH"
            }

            var helper = Dependencies.SqlGenerationHelper;

            builder.Append("ADD SENSITIVITY CLASSIFICATION TO ")
                .Append(helper.DelimitIdentifier(tableName, schemaName))
                .Append(".")
                .Append(helper.DelimitIdentifier(columnName))
                .AppendLine()
                .Append("WITH (");

            var first = true;

            if (!string.IsNullOrWhiteSpace(label)) {
                builder.Append("LABEL = N'")
                       .Append(label.Replace("'", "''"))
                       .Append("'");
                first = false;
            }

            if (!string.IsNullOrWhiteSpace(informationType)) {
                if (!first) builder.Append(", ");
                builder.Append("INFORMATION_TYPE = N'")
                       .Append(informationType.Replace("'", "''"))
                       .Append("'");
                first = false;
            }

            if (!string.IsNullOrWhiteSpace(sqlRank)) {
                if (!first) builder.Append(", ");
                builder.Append("RANK = ")
                       .Append(sqlRank);
            }

            builder.Append(");")
                   .EndCommand();
        }
    }
}
