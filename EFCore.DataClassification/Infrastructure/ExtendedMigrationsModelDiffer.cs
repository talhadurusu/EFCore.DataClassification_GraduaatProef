using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.DataClassification.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;

#pragma warning disable EF1001

namespace EFCore.DataClassification.Infrastructure {
    /// <summary>
    /// Detects changes in DataClassification annotations and generates
    /// AlterColumnOperation when needed for migration system.
    /// </summary>
    public sealed class DataClassificationMigrationsModelDiffer : MigrationsModelDiffer {
        public DataClassificationMigrationsModelDiffer(
            IRelationalTypeMappingSource typeMappingSource,
            IMigrationsAnnotationProvider migrationsAnnotationProvider,
            IRowIdentityMapFactory rowIdentityMapFactory,
            CommandBatchPreparerDependencies commandBatchPreparerDependencies)
            : base(
                typeMappingSource,
                migrationsAnnotationProvider,
                rowIdentityMapFactory,
                commandBatchPreparerDependencies) {
        }
#pragma warning restore EF1001 // Internal EF Core API usage.

        protected override IEnumerable<MigrationOperation> Diff(
            IColumn source,
            IColumn target,
            DiffContext diffContext) {
            var baseOps = base.Diff(source, target, diffContext).ToList();

            // If EF already generated an AlterColumn, don't add another one
            if (baseOps.OfType<AlterColumnOperation>().Any())
                return baseOps;

            // Only proceed if DataClassification annotations changed
            if (!HasDataClassificationChanged(source, target))
                return baseOps;

            var targetProperty = target.PropertyMappings.FirstOrDefault()?.Property;
            if (targetProperty is null)
                return baseOps;

            baseOps.Add(CreateAlterColumnOperation(source, target, targetProperty));
            return baseOps;
        }

        private static AlterColumnOperation CreateAlterColumnOperation(
            IColumn source,
            IColumn target,
            IProperty targetProperty) {
            var sourceProperty = source.PropertyMappings.FirstOrDefault()?.Property;
            var newClrType = GetSafeClrType(targetProperty.ClrType);
            var oldClrType = GetSafeClrType(sourceProperty?.ClrType ?? targetProperty.ClrType);

            return new AlterColumnOperation {
                Schema = target.Table.Schema,
                Table = target.Table.Name,
                Name = target.Name,

                // Current column definition
                ClrType = newClrType,
                ColumnType = target.StoreType ?? targetProperty.GetColumnType(),
                IsNullable = target.IsNullable,
                MaxLength = target.MaxLength,
                Precision = target.Precision,
                Scale = target.Scale,
                IsUnicode = target.IsUnicode,
                IsRowVersion = target.IsRowVersion,

                // Previous column definition for migration comparison
                OldColumn = new AddColumnOperation {
                    ClrType = oldClrType,
                    ColumnType = source.StoreType ?? sourceProperty?.GetColumnType(),
                    IsNullable = source.IsNullable,
                    MaxLength = source.MaxLength,
                    Precision = source.Precision,
                    Scale = source.Scale,
                    IsUnicode = source.IsUnicode,
                    IsRowVersion = source.IsRowVersion
                }
            };
        }

        /// <summary>
        /// Ensures CLR type is safe for migration operations.
        /// Falls back to string type for null or namespace-less types.
        /// </summary>
        private static Type GetSafeClrType(Type? type)
            => type == null || type.Namespace == null ? typeof(string) : type;

        private static bool HasDataClassificationChanged(IColumn source, IColumn target) {
            var sProp = source.PropertyMappings.FirstOrDefault()?.Property;
            var tProp = target.PropertyMappings.FirstOrDefault()?.Property;

            if (sProp is null || tProp is null)
                return false;

            return HasAnnotationChanged(sProp, tProp, DataClassificationConstants.Label)
                || HasAnnotationChanged(sProp, tProp, DataClassificationConstants.InformationType)
                || HasAnnotationChanged(sProp, tProp, DataClassificationConstants.Rank);
        }

        private static bool HasAnnotationChanged(IProperty source, IProperty target, string annotationKey) {
            var sourceValue = source.FindAnnotation(annotationKey)?.Value?.ToString() ?? string.Empty;
            var targetValue = target.FindAnnotation(annotationKey)?.Value?.ToString() ?? string.Empty;
            return sourceValue != targetValue;
        }
    }
}