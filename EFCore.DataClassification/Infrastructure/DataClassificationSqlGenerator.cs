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
    /// SQL generator that produces both custom extended properties
    /// and SQL Server's native sensitivity classification metadata
    /// (ADD SENSITIVITY CLASSIFICATION) based on DataClassification annotations.
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
                    remove.Schema ?? DataClassificationConstants.DefaultSchema,
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
            // First execute normal ALTER TABLE ADD
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
            var schemaName = operation.Schema ?? DataClassificationConstants.DefaultSchema;

            
            ClearDataClassification(builder,schemaName,operation.Table,operation.Name);

       
            base.Generate(operation,model,builder,terminate);
        }

        #endregion


        #region Orchestrator helpers (Write / Clear)
        
        /// <summary>
        /// Writes data classification from EF Core property annotations
        /// </summary>
        private void WriteDataClassification(MigrationCommandListBuilder builder,string? schema,string tableName,string columnName,IProperty property) {

            var schemaName = schema ?? DataClassificationConstants.DefaultSchema;

            // Extract annotation values
            var label = property.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString();
            var informationType = property.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString();
            var rank = property.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString();

            // Validate before processing
            ValidateDataClassification(property,label,informationType,rank);

            // Delegate to core implementation
            WriteDataClassificationCore(builder, schemaName, tableName, columnName, label, informationType, rank);
        }

        /// <summary>
        /// Writes data classification from explicit string parameters
        /// </summary>
        /// <remarks>
        /// Used for custom migration operations (CreateDataClassificationOperation)
        /// </remarks>
        private void WriteDataClassification(
           MigrationCommandListBuilder builder,
           string? schema,
           string table,
           string column,
           string? label,
           string? informationType,
           string? rank,
           string? propertyDisplayName) {

            var schemaName = schema ?? DataClassificationConstants.DefaultSchema;
            var targetName = propertyDisplayName ?? $"{schemaName}.{table}.{column}";

            // Validate before processing
            ValidateDataClassification(targetName, label, informationType, rank);

            // Delegate to core implementation
            WriteDataClassificationCore(builder, schemaName, table, column, label, informationType, rank);
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

            var nameLiteral = stringMapping.GenerateSqlLiteral(propertyName);
            var schemaLiteral = stringMapping.GenerateSqlLiteral(schemaName);
            var tableLiteral = stringMapping.GenerateSqlLiteral(tableName);
            var columnLiteral = stringMapping.GenerateSqlLiteral(columnName);
            var valueLiteral = stringMapping.GenerateSqlLiteral(propertyValue);

            builder
                .AppendLine(
                    $"""
             EXEC sys.sp_addextendedproperty
                 @name = {nameLiteral},
                 @value = {valueLiteral},
                 @level0type = N'SCHEMA', @level0name = {schemaLiteral},
                 @level1type = N'TABLE',  @level1name = {tableLiteral},
                 @level2type = N'COLUMN', @level2name = {columnLiteral};
             """)
                .EndCommand();
        }


        private void AppendDropExtendedProperty(MigrationCommandListBuilder builder,string schemaName,string tableName,string columnName,string propertyName) {

            var helper = Dependencies.SqlGenerationHelper;
            var stringMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            var fullName = helper.DelimitIdentifier(tableName, schemaName);
            var fullNameLiteral = stringMapping.GenerateSqlLiteral(fullName);
            var columnLiteral = stringMapping.GenerateSqlLiteral(columnName);
            var nameLiteral = stringMapping.GenerateSqlLiteral(propertyName);
            var schemaLiteral = stringMapping.GenerateSqlLiteral(schemaName);
            var tableLiteral = stringMapping.GenerateSqlLiteral(tableName);

            builder
                .AppendLine(
                    $"""
             IF EXISTS (
                 SELECT 1
                 FROM sys.extended_properties ep
                 WHERE ep.name = {nameLiteral}
                   AND ep.major_id = OBJECT_ID({fullNameLiteral})
                   AND ep.minor_id = COLUMNPROPERTY(OBJECT_ID({fullNameLiteral}), {columnLiteral}, 'ColumnId')
             )
                 EXEC sys.sp_dropextendedproperty
                     @name = {nameLiteral},
                     @level0type = N'SCHEMA', @level0name = {schemaLiteral},
                     @level1type = N'TABLE',  @level1name = {tableLiteral},
                     @level2type = N'COLUMN', @level2name = {columnLiteral};
             """)
                .EndCommand();
        }


        #endregion


        #region Sensitivity classification helpers
        private void AppendDropSensitivityClassification(MigrationCommandListBuilder builder,string schemaName,string tableName,string columnName) {
            var helper = Dependencies.SqlGenerationHelper;
            var stringMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

            var objectIdName = $"{schemaName}.{tableName}";
            var objectIdLiteral = stringMapping.GenerateSqlLiteral(objectIdName);
            var columnLiteral = stringMapping.GenerateSqlLiteral(columnName);

            var delimitedTable = helper.DelimitIdentifier(tableName, schemaName);
            var delimitedColumn = helper.DelimitIdentifier(columnName);

            builder.AppendLine(
                $"""
         IF EXISTS (
             SELECT 1
             FROM sys.sensitivity_classifications sc
             WHERE sc.major_id = OBJECT_ID({objectIdLiteral})
               AND sc.minor_id = COLUMNPROPERTY(OBJECT_ID({objectIdLiteral}), {columnLiteral}, 'ColumnId')
         )
             DROP SENSITIVITY CLASSIFICATION FROM {delimitedTable}.{delimitedColumn};
         """)
                .EndCommand();
        }


        private void AppendSensitivityClassification(MigrationCommandListBuilder builder,string schemaName,string tableName, string columnName,string? label,string? informationType,string? rankString) {

            
            if (string.IsNullOrWhiteSpace(label)&& string.IsNullOrWhiteSpace(informationType)&& string.IsNullOrWhiteSpace(rankString)) {
                return;
            }

            string? sqlRank = null;
            if (!string.IsNullOrWhiteSpace(rankString)) {
                sqlRank = rankString switch {
                    "None" => null,  
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

            static string GetEntityName(IProperty p) {
                if (p.DeclaringType is IEntityType entityType) {
                    return entityType.DisplayName();
                }

                return p.DeclaringType.Name;
            }

            if (!string.IsNullOrWhiteSpace(rank) && !DataClassificationConstants.IsValidRank(rank)) {
                var entityName = GetEntityName(property);
                var propertyName = property.Name;

                throw new DataClassificationException(
                    property,
                    $"Invalid DataClassification Rank '{rank}' on property '{entityName}.{propertyName}'. " +
                    $"Allowed values: {DataClassificationConstants.GetAllowedRanksString()}.");
            }

            if (label?.Length > DataClassificationConstants.MaxLabelLength) {
                var entityName = GetEntityName(property);
                var propertyName = property.Name;

                throw new DataClassificationException(
                    property,
                    $"DataClassification Label on '{entityName}.{propertyName}' is too long " +
                    $"({label.Length} chars, max {DataClassificationConstants.MaxLabelLength}).");
            }
        }

        private static void ValidateDataClassification(string targetName, string? label, string? informationType, string? rank) {
            if (string.IsNullOrWhiteSpace(label)
                && string.IsNullOrWhiteSpace(informationType)
                && string.IsNullOrWhiteSpace(rank)) {
                return;
            }

            if (!string.IsNullOrWhiteSpace(rank) && !DataClassificationConstants.IsValidRank(rank)) {
                throw new DataClassificationException(
                    $"Invalid DataClassification Rank '{rank}' on property '{targetName}'. " +
                    $"Allowed values: {DataClassificationConstants.GetAllowedRanksString()}.");
            }

            if (label?.Length > DataClassificationConstants.MaxLabelLength) {
                throw new DataClassificationException(
                    $"DataClassification Label on '{targetName}' is too long " +
                    $"({label.Length} chars, max {DataClassificationConstants.MaxLabelLength}).");
            }
        }


        /// <summary>
        /// Core logic for writing data classification metadata.
        /// Both public overloads delegate to this method to avoid code duplication.
        /// </summary>
        /// <remarks>
        /// This method:
        /// 1. Checks if any classification data exists (early return if empty)
        /// 2. Writes extended properties for each classification component
        /// 3. Writes SQL Server native sensitivity classification
        /// </remarks>
        private void WriteDataClassificationCore(
            MigrationCommandListBuilder builder,
            string schemaName,
            string tableName,
            string columnName,
            string? label,
            string? informationType,
            string? rank) {

            // Early return if no classification data
            if (string.IsNullOrWhiteSpace(label)
                && string.IsNullOrWhiteSpace(informationType)
                && string.IsNullOrWhiteSpace(rank)) {
                return;
            }

            // Write extended properties
            if (!string.IsNullOrWhiteSpace(label)) {
                AppendExtendedProperty(
                    builder, schemaName, tableName, columnName,
                    DataClassificationConstants.Label, label);
            }

            if (!string.IsNullOrWhiteSpace(informationType)) {
                AppendExtendedProperty(
                    builder, schemaName, tableName, columnName,
                    DataClassificationConstants.InformationType, informationType);
            }

            if (!string.IsNullOrWhiteSpace(rank)) {
                AppendExtendedProperty(
                    builder, schemaName, tableName, columnName,
                    DataClassificationConstants.Rank, rank);
            }

            // Write SQL Server sensitivity classification
            AppendSensitivityClassification(
                builder, schemaName, tableName, columnName,
                label, informationType, rank);
        }

        #endregion
    }
}
