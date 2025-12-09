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


        #region Orchestrator helpers (Write / Clear)
     
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

        #region Core logic
        /// <summary>
        /// Core logic for writing data classification metadata..
        /// </summary>
        /// <remarks>
        /// This method:
        /// 1. Checks if any classification data exists (early return if empty)
        /// 2. Writes extended properties for each classification component
        /// 3. Writes SQL Server native sensitivity classification
        /// </remarks>
        /// 
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



        
    }
}
