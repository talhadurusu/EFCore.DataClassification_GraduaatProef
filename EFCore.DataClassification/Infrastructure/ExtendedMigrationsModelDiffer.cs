using System;
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
    /// Adds/removes SQL Server Data Classification metadata during migrations,
    /// based on model annotations found on mapped properties.
    /// </summary>
    public sealed class DataClassificationMigrationsModelDiffer : MigrationsModelDiffer {
        #region Constructor

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

        #endregion

        #region Overrides

        protected override IEnumerable<MigrationOperation> Add(ITable target, DiffContext diffContext) {
            var ops = base.Add(target, diffContext).ToList();

            foreach (var column in target.Columns) {
                var prop = GetMappedProperty(column);
                if (prop is null)
                    continue;

                if (HasClassification(prop))
                    ops.Add(GenerateCreateOperation(column, prop));
            }

            return ops;
        }

        protected override IEnumerable<MigrationOperation> Diff(ITable source, ITable target, DiffContext diffContext) {
            var ops = base.Diff(source, target, diffContext).ToList();

            var sourceByName = source.Columns.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var targetByName = target.Columns.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

            // Added columns
            foreach (var (name, targetColumn) in targetByName) {
                if (sourceByName.ContainsKey(name))
                    continue;

                var prop = GetMappedProperty(targetColumn);
                if (prop is not null && HasClassification(prop))
                    ops.Add(GenerateCreateOperation(targetColumn, prop));
            }

            // Removed columns
            foreach (var (name, sourceColumn) in sourceByName) {
                if (targetByName.ContainsKey(name))
                    continue;

                var prop = GetMappedProperty(sourceColumn);
                if (prop is not null && HasClassification(prop))
                    ops.Add(GenerateRemoveOperation(sourceColumn));
            }

            return ops;
        }

        protected override IEnumerable<MigrationOperation> Diff(IColumn source, IColumn target, DiffContext diffContext) {
            var ops = base.Diff(source, target, diffContext).ToList();

            var sProp = GetMappedProperty(source);
            var tProp = GetMappedProperty(target);

            if (sProp is null && tProp is null)
                return ops;

            var sHas = sProp is not null && HasClassification(sProp);
            var tHas = tProp is not null && HasClassification(tProp);

            // Mapping removed
            if (sProp is not null && tProp is null) {
                if (sHas)
                    ops.Add(GenerateRemoveOperation(source));
                return ops;
            }

            // Mapping added
            if (sProp is null && tProp is not null) {
                if (tHas)
                    ops.Add(GenerateCreateOperation(target, tProp));
                return ops;
            }

            // Both mapped
            if (sHas && !tHas) {
                ops.Add(GenerateRemoveOperation(target));
                return ops;
            }

            if (!sHas && tHas) {
                ops.Add(GenerateCreateOperation(target, tProp!));
                return ops;
            }

            if (sHas && tHas && HasDataClassificationChanged(sProp!, tProp!)) {
                ops.Add(GenerateRemoveOperation(target));
                ops.Add(GenerateCreateOperation(target, tProp!));
            }

            return ops;
        }

        protected override IReadOnlyList<MigrationOperation> Sort(IEnumerable<MigrationOperation> operations, DiffContext diffContext) {
            var sorted = base.Sort(operations, diffContext).ToList();

            for (var i = 0; i < sorted.Count; i++) {
                if (sorted[i] is not DropColumnOperation drop)
                    continue;

                var removeIdx = sorted.FindIndex(op =>
                    op is RemoveDataClassificationOperation remove
                    && string.Equals(remove.Schema ?? drop.Schema, drop.Schema, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(remove.Table, drop.Table, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(remove.Column, drop.Name, StringComparison.OrdinalIgnoreCase));

                if (removeIdx >= 0 && removeIdx > i) {
                    var remove = sorted[removeIdx];
                    sorted.RemoveAt(removeIdx);
                    sorted.Insert(i, remove);
                }
            }

            return sorted;
        }

        #endregion

        #region Helpers

        private static IProperty? GetMappedProperty(IColumn column)
            => column.PropertyMappings.FirstOrDefault()?.Property;

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
            if (property.DeclaringType is IEntityType entityType)
                return $"{entityType.DisplayName()}.{property.Name}";

            return $"{property.DeclaringType.Name}.{property.Name}";
        }

        private static bool HasDataClassificationChanged(IProperty sourceProp, IProperty targetProp)
            => HasAnnotationChanged(sourceProp, targetProp, DataClassificationConstants.Label)
               || HasAnnotationChanged(sourceProp, targetProp, DataClassificationConstants.InformationType)
               || HasAnnotationChanged(sourceProp, targetProp, DataClassificationConstants.Rank);

        private static bool HasAnnotationChanged(IProperty source, IProperty target, string annotationKey) {
            var sourceValue = source.FindAnnotation(annotationKey)?.Value?.ToString() ?? string.Empty;
            var targetValue = target.FindAnnotation(annotationKey)?.Value?.ToString() ?? string.Empty;
            return !string.Equals(sourceValue, targetValue, StringComparison.Ordinal);
        }

        #endregion
    }
}

#pragma warning restore EF1001
