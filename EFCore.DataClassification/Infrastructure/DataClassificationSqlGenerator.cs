using EFCore.DataClassification.Annotations;
using EFCore.DataClassification.Exceptions;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;

namespace EFCore.DataClassification.Infrastructure {
    /// <summary>
    /// DataClassification annotation'larına göre
    /// hem kendi extended property'lerimizi,
    /// hem de SQL Server'ın resmi "sensitivity classification"
    /// metadata'sını (ADD SENSITIVITY CLASSIFICATION) üreten generator.
    /// </summary>
    public sealed class DataClassificationSqlGenerator : SqlServerMigrationsSqlGenerator {

        #region Ctor
        public DataClassificationSqlGenerator(
            MigrationsSqlGeneratorDependencies dependencies,
            ICommandBatchPreparer commandBatchPreparer)
            : base(dependencies, commandBatchPreparer) {
        }
        #endregion

        protected override void Generate(MigrationOperation operation,IModel? model,MigrationCommandListBuilder builder) {

            switch (operation) {
                case CreateDataClassificationOperation create:
                    WriteDataClassification(
                        builder,
                        create.Schema,
                        create.Table,
                        create.Column,
                        create.Label,
                        create.InformationType,
                        create.Rank,
                        create.PropertyDisplayName);
                    return;

                case RemoveDataClassificationOperation remove:
                    ClearDataClassification(
                        builder,
                        remove.Schema ?? "dbo",
                        remove.Table,
                        remove.Column);
                    return;
            }

            base.Generate(operation, model, builder);
        }

        #region Generate overrides (Create / Add / Alter / Drop)

        // 1) CREATE TABLE


        protected override void Generate(CreateTableOperation operation,IModel? model,MigrationCommandListBuilder builder,bool terminate = true) {
         
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

        
        // 2) ADD COLUMN
    
        protected override void Generate(AddColumnOperation operation, IModel? model,MigrationCommandListBuilder builder,bool terminate = true) {
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

        // 4) DROP COLUMN
     
        protected override void Generate(DropColumnOperation operation,IModel? model,MigrationCommandListBuilder builder,bool terminate = true) {
            var schemaName = operation.Schema ?? "dbo";

            
            ClearDataClassification(builder,schemaName,operation.Table,operation.Name);

       
            base.Generate(operation,model,builder,terminate);
        }

        #endregion


        #region Orchestrator helpers (Write / Clear)
        
        private void WriteDataClassification(MigrationCommandListBuilder builder,string? schema,string tableName,string columnName,IProperty property) {

            var schemaName = schema ?? "dbo";

            // Annotation'lardan değerleri oku
            var label = property.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString();
            var infoType = property.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString();
            var rank = property.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString();

            ValidateDataClassification(property,label,infoType,rank);

            // Hiç veri yoksa, bu kolon için classification yok demektir
            if (string.IsNullOrWhiteSpace(label)
                && string.IsNullOrWhiteSpace(infoType)
                && string.IsNullOrWhiteSpace(rank)) {
                return;
            }

            // 1) Senin kendi extended property'lerin (DataClassification:*)
            if (!string.IsNullOrWhiteSpace(label)) {
                AppendExtendedProperty(
                    builder,schemaName,tableName,columnName,
                    DataClassificationConstants.Label, label);
            }

            if (!string.IsNullOrWhiteSpace(infoType)) {
                AppendExtendedProperty(
                    builder,schemaName,tableName,columnName,
                    DataClassificationConstants.InformationType,infoType);
            }

            if (!string.IsNullOrWhiteSpace(rank)) {
                AppendExtendedProperty(
                    builder,schemaName,tableName,columnName,
                    DataClassificationConstants.Rank,rank);
            }

            // 2) SQL Server sensitivity classification
            AppendSensitivityClassification(
                builder,schemaName,tableName,columnName,
                label,infoType,rank);
        }

        private void WriteDataClassification(
           MigrationCommandListBuilder builder,
           string? schema,
           string table,
           string column,
           string? label,
           string? informationType,
           string? rank,
           string? propertyDisplayName) {

            var schemaName = schema ?? "dbo";
            var targetName = propertyDisplayName ?? $"{schemaName}.{table}.{column}";

            ValidateDataClassification(targetName, label, informationType, rank);

            if (string.IsNullOrWhiteSpace(label)
                && string.IsNullOrWhiteSpace(informationType)
                && string.IsNullOrWhiteSpace(rank)) {
                return;
            }

            if (!string.IsNullOrWhiteSpace(label)) {
                AppendExtendedProperty(builder, schemaName, table, column, DataClassificationConstants.Label, label);
            }

            if (!string.IsNullOrWhiteSpace(informationType)) {
                AppendExtendedProperty(builder, schemaName, table, column, DataClassificationConstants.InformationType, informationType);
            }

            if (!string.IsNullOrWhiteSpace(rank)) {
                AppendExtendedProperty(builder, schemaName, table, column, DataClassificationConstants.Rank, rank);
            }

            AppendSensitivityClassification(builder, schemaName, table, column, label, informationType, rank);
        }

        private void ClearDataClassification(MigrationCommandListBuilder builder,string schemaName,string tableName,string columnName) {
            // Extended properties
            AppendDropExtendedProperty(
                builder, schemaName, tableName, columnName, DataClassificationConstants.Label);

            AppendDropExtendedProperty(
                builder, schemaName, tableName, columnName, DataClassificationConstants.InformationType);

            AppendDropExtendedProperty(
                builder, schemaName, tableName, columnName, DataClassificationConstants.Rank);

            // Sensitivity classification
            AppendDropSensitivityClassification(
                builder, schemaName, tableName, columnName);
        }

        #endregion


        #region Extended property helpers

        private void AppendExtendedProperty(MigrationCommandListBuilder builder,string schemaName,string tableName,string columnName,string propertyName,string propertyValue) {

            var stringMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
            var safeValue = stringMapping.GenerateSqlLiteral(propertyValue);

          


            builder
                .AppendLine(
                    $"""
                     EXEC sys.sp_addextendedproperty
                         @name = N'{propertyName}',
                         @value = {safeValue},
                         @level0type = N'SCHEMA', @level0name = N'{schemaName}',
                         @level1type = N'TABLE',  @level1name = N'{tableName}',
                         @level2type = N'COLUMN', @level2name = N'{columnName}';
                     """)
                .EndCommand();
        }

        private void AppendDropExtendedProperty(MigrationCommandListBuilder builder,string schemaName,string tableName,string columnName,string propertyName) {
            
            
            var helper = Dependencies.SqlGenerationHelper;
            var fullName = helper.DelimitIdentifier(tableName, schemaName);

            builder
                .AppendLine(
                    $"""
                     IF EXISTS (
                         SELECT 1
                         FROM sys.extended_properties ep
                         WHERE ep.name = N'{propertyName}'
                           AND ep.major_id = OBJECT_ID(N'{fullName}')
                           AND ep.minor_id = COLUMNPROPERTY(OBJECT_ID(N'{fullName}'), N'{columnName}', 'ColumnId')
                     )
                         EXEC sys.sp_dropextendedproperty
                             @name = N'{propertyName}',
                             @level0type = N'SCHEMA', @level0name = N'{schemaName}',
                             @level1type = N'TABLE',  @level1name = N'{tableName}',
                             @level2type = N'COLUMN', @level2name = N'{columnName}';
                     """)
                .EndCommand();
        }

        #endregion


        #region Sensitivity classification helpers
        private void AppendDropSensitivityClassification(MigrationCommandListBuilder builder,string schemaName,string tableName,string columnName) {
            var helper = Dependencies.SqlGenerationHelper;

            // OBJECT_ID için parantezsiz schema.table formu kullanmak daha sağlıklı
            var objectIdName = $"{schemaName}.{tableName}";

            // DDL tarafı için yine DelimitIdentifier
            var delimitedTable = helper.DelimitIdentifier(tableName, schemaName);
            var delimitedColumn = helper.DelimitIdentifier(columnName);

            builder
                .AppendLine(
                    $"""
             IF EXISTS (
                 SELECT 1
                 FROM sys.sensitivity_classifications sc
                 WHERE sc.major_id = OBJECT_ID(N'{objectIdName}')
                   AND sc.minor_id = COLUMNPROPERTY(OBJECT_ID(N'{objectIdName}'), N'{columnName}', 'ColumnId')
             )
                 DROP SENSITIVITY CLASSIFICATION FROM {delimitedTable}.{delimitedColumn};
             """)
                .EndCommand();
        }


        private void AppendSensitivityClassification(MigrationCommandListBuilder builder,string schemaName,string tableName, string columnName,string? label,string? informationType,string? rankString) {

            // Hiç veri yoksa boşuna SQL üretme
            if (string.IsNullOrWhiteSpace(label)&& string.IsNullOrWhiteSpace(informationType)&& string.IsNullOrWhiteSpace(rankString)) {
                return;
            }

            string? sqlRank = null;
            if (!string.IsNullOrWhiteSpace(rankString)) {
                sqlRank = rankString switch {
                    "Low" => "LOW",
                    "Medium" => "MEDIUM",
                    "High" => "HIGH",
                    "Critical" => "CRITICAL",
                    _ => "LOW"
                };
            }

            var helper = Dependencies.SqlGenerationHelper;
            var stringMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));
            var fullName = helper.DelimitIdentifier(tableName, schemaName);
            var delimitedColumn = helper.DelimitIdentifier(columnName);

            builder
                .Append($"ADD SENSITIVITY CLASSIFICATION TO {fullName}.{delimitedColumn}")
                .AppendLine()
                .Append("WITH (");

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(label)) {
                var safeLabel = stringMapping.GenerateSqlLiteral(label);
                parts.Add($"LABEL = {safeLabel}");
            }

            if (!string.IsNullOrWhiteSpace(informationType)) {
                var safeInfoType = stringMapping.GenerateSqlLiteral(informationType);
                parts.Add($"INFORMATION_TYPE = {safeInfoType}");
            }

            if (!string.IsNullOrWhiteSpace(sqlRank)) {
                parts.Add($"RANK = {sqlRank}");
            }

            builder.Append(string.Join(", ", parts));
            builder.Append(");")
                   .EndCommand();
        }

        #endregion

        #region Validation

        private static void ValidateDataClassification(IProperty property, string? label, string? informationType, string? rank) {
            if (string.IsNullOrWhiteSpace(label)
                && string.IsNullOrWhiteSpace(informationType)
                && string.IsNullOrWhiteSpace(rank)) {
                return;
            }

            var allowedRanks = new[] { "None", "Low", "Medium", "High", "Critical" };

            static string GetEntityName(IProperty p) {
                if (p.DeclaringType is IEntityType entityType) {
                    return entityType.DisplayName();
                }

                return p.DeclaringType.Name;
            }

            if (!string.IsNullOrWhiteSpace(rank) && !allowedRanks.Contains(rank)) {
                var entityName = GetEntityName(property);
                var propertyName = property.Name;

                throw new DataClassificationException(
                    property,
                    $"Invalid DataClassification Rank '{rank}' on property '{entityName}.{propertyName}'. " +
                    $"Allowed values: {string.Join(", ", allowedRanks)}.");
            }

            if (label?.Length > 128) {
                var entityName = GetEntityName(property);
                var propertyName = property.Name;

                throw new DataClassificationException(
                    property,
                    $"DataClassification Label on '{entityName}.{propertyName}' is too long " +
                    $"{label.Length} chars, max 128.");
            }
        }

        private static void ValidateDataClassification(string targetName, string? label, string? informationType, string? rank) {
            if (string.IsNullOrWhiteSpace(label)
                && string.IsNullOrWhiteSpace(informationType)
                && string.IsNullOrWhiteSpace(rank)) {
                return;
            }

            var allowedRanks = new[] { "None", "Low", "Medium", "High", "Critical" };

            if (!string.IsNullOrWhiteSpace(rank) && !allowedRanks.Contains(rank)) {
                throw new DataClassificationException(
                    $"Invalid DataClassification Rank '{rank}' on property '{targetName}'. Allowed values: {string.Join(", ", allowedRanks)}.");
            }

            if (label?.Length > 128) {
                throw new DataClassificationException(
                    $"DataClassification Label on '{targetName}' is too long ({label.Length} chars, max 128).");
            }
        }


        #endregion
    }
}
