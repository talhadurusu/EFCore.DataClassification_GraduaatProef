using System.Collections.Generic;
using System.Linq;
using EFCore.DataClassification.Annotations;
using EFCore.DataClassification.Operations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;

#pragma warning disable EF1001

namespace EFCore.DataClassification.Infrastructure {
    /// <summary>
    /// Detects DataClassification annotations and generates
    /// CreateDataClassificationOperation / RemoveDataClassificationOperation when needed.
    ///
    /// Handles:
    /// - New tables (Add ITable)
    /// - New columns (Diff ITable: target-only columns)
    /// - Column changes (Diff IColumn)
    /// - Column removal (Diff ITable: source-only columns)
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

        // NEW TABLE
        protected override IEnumerable<MigrationOperation> Add(ITable target, DiffContext diffContext) {
            var ops = base.Add(target, diffContext).ToList();

            foreach (var column in target.Columns) {
                var prop = column.PropertyMappings.FirstOrDefault()?.Property;
                if (prop is null)
                    continue;

                if (HasClassification(prop)) {
                    ops.Add(GenerateCreateOperation(column, prop));
                }
            }

            return ops;
        }

        // TABLE DIFF: detect new/removed columns 
        protected override IEnumerable<MigrationOperation> Diff(
            ITable source,
            ITable target,
            DiffContext diffContext) {
            var ops = base.Diff(source, target, diffContext).ToList();

            // Newly added columns: present in target, absent in source
            foreach (var targetColumn in target.Columns) {
                var sourceColumn = source.Columns.FirstOrDefault(c => c.Name == targetColumn.Name);
                if (sourceColumn is not null)
                    continue;

                var prop = targetColumn.PropertyMappings.FirstOrDefault()?.Property;
                if (prop is not null && HasClassification(prop)) {
                    ops.Add(GenerateCreateOperation(targetColumn, prop));
                }
            }

            // Deleted columns: present in source, absent in target
            foreach (var sourceColumn in source.Columns) {
                var targetColumn = target.Columns.FirstOrDefault(c => c.Name == sourceColumn.Name);
                if (targetColumn is not null)
                    continue;

                var prop = sourceColumn.PropertyMappings.FirstOrDefault()?.Property;
                if (prop is null || !HasClassification(prop))
                    continue;

                ops.Add(GenerateRemoveOperation(sourceColumn));
            }

            return ops;
        }

        // EXISTING COLUMN CHANGES
        protected override IEnumerable<MigrationOperation> Diff(
     IColumn source,
     IColumn target,
     DiffContext diffContext) {
            var baseOps = base.Diff(source, target, diffContext).ToList();

            var sourceProp = source.PropertyMappings.FirstOrDefault()?.Property;
            var targetProp = target.PropertyMappings.FirstOrDefault()?.Property;

            if (sourceProp is null && targetProp is null)
                return baseOps;

           
            if (sourceProp != null && targetProp is null) {
                baseOps.Add(GenerateRemoveOperation(source));
                return baseOps;
            }

            // Column gained a mapped property with classification => create metadata
            if (sourceProp is null && targetProp != null) {
                baseOps.Add(GenerateCreateOperation(target, targetProp));
                return baseOps;
            }

            // Both have properties; check if classification changed
            if (!HasDataClassificationChanged(source, target) || targetProp is null)
                return baseOps;

            baseOps.Add(GenerateRemoveOperation(target));
            baseOps.Add(GenerateCreateOperation(target, targetProp));
            return baseOps;
        }
        protected override IReadOnlyList<MigrationOperation> Sort(
            IEnumerable<MigrationOperation> operations,
            DiffContext diffContext) {
            var sorted = base.Sort(operations, diffContext).ToList();

            for (var i = 0; i < sorted.Count; i++) {
                if (sorted[i] is DropColumnOperation drop) {
                    var removeIdx = sorted.FindIndex(op =>
                        op is RemoveDataClassificationOperation remove &&
                        string.Equals(remove.Schema ?? drop.Schema, drop.Schema, StringComparison.OrdinalIgnoreCase) &&
                        remove.Table == drop.Table &&
                        remove.Column == drop.Name);

                    if (removeIdx >= 0 && removeIdx > i) {
                        var remove = sorted[removeIdx];
                        sorted.RemoveAt(removeIdx);
                        sorted.Insert(i, remove);
                    }
                }
            }

            return sorted;
        }

        // Helpers --------------------------------------------------------------

        private static bool HasClassification(IProperty property) {
            var label = GetAnnotation(property, DataClassificationConstants.Label);
            var infoType = GetAnnotation(property, DataClassificationConstants.InformationType);
            var rank = GetAnnotation(property, DataClassificationConstants.Rank);

            return !string.IsNullOrWhiteSpace(label)
                   || !string.IsNullOrWhiteSpace(infoType)
                   || !string.IsNullOrWhiteSpace(rank);
        }

        private static CreateDataClassificationOperation GenerateCreateOperation(IColumn column, IProperty property)
            => new() {
                Schema = column.Table.Schema,
                Table = column.Table.Name,
                Column = column.Name,
                Label = GetAnnotation(property, DataClassificationConstants.Label),
                InformationType = GetAnnotation(property, DataClassificationConstants.InformationType),
                Rank = GetAnnotation(property, DataClassificationConstants.Rank),
                PropertyDisplayName = GetPropertyDisplayName(property)
            };

        private static RemoveDataClassificationOperation GenerateRemoveOperation(IColumn column)
            => new() {
                Schema = column.Table.Schema,
                Table = column.Table.Name,
                Column = column.Name
            };

        private static string? GetAnnotation(IProperty property, string key)
            => property.FindAnnotation(key)?.Value?.ToString();

        private static string GetPropertyDisplayName(IProperty property) {
            if (property.DeclaringType is IEntityType entityType) {
                return $"{entityType.DisplayName()}.{property.Name}";
            }

            return $"{property.DeclaringType.Name}.{property.Name}";
        }

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

#pragma warning restore EF1001
