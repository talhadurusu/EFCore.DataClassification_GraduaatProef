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
                AddCreateOperationIfNeeded(ops, column);
            }

            return ops;
        }

        /// <summary>
        /// Handles table-level diffs. Tracks renamed columns to avoid duplicate classification operations.
        /// </summary>
        protected override IEnumerable<MigrationOperation> Diff(ITable source, ITable target, DiffContext diffContext) {
            var ops = base.Diff(source, target, diffContext).ToList();

            var renameOps = ops.OfType<RenameColumnOperation>().ToList();
            HashSet<string>? renamedOldNames = null;
            HashSet<string>? renamedNewNames = null;
            
            if (renameOps.Count > 0) {
                renamedOldNames = new HashSet<string>(renameOps.Select(r => r.Name), StringComparer.OrdinalIgnoreCase);
                renamedNewNames = new HashSet<string>(renameOps.Select(r => r.NewName), StringComparer.OrdinalIgnoreCase);
            }

            var sourceByName = source.Columns.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
            var targetByName = target.Columns.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var (name, targetColumn) in targetByName) {
                if (sourceByName.ContainsKey(name))
                    continue;
                
                if (renamedNewNames?.Contains(name) == true)
                    continue;

                AddCreateOperationIfNeeded(ops, targetColumn);
            }

            foreach (var (name, sourceColumn) in sourceByName) {
                if (targetByName.ContainsKey(name))
                    continue;
                
                if (renamedOldNames?.Contains(name) == true)
                    continue;

                AddRemoveOperationIfNeeded(ops, sourceColumn);
            }

            return ops;
        }

        /// <summary>
        /// Handles column-level diffs. Detects classification changes, additions, removals, and column renames.
        /// </summary>
        protected override IEnumerable<MigrationOperation> Diff(IColumn source, IColumn target, DiffContext diffContext) {
            var ops = base.Diff(source, target, diffContext).ToList();

            var sourceProperty = GetMappedProperty(source);
            var targetProperty = GetMappedProperty(target);

            if (sourceProperty is null && targetProperty is null)
                return ops;

            var sourceHasClassification = sourceProperty is not null && HasClassification(sourceProperty);
            var targetHasClassification = targetProperty is not null && HasClassification(targetProperty);

            var isRenamed = !string.Equals(source.Name, target.Name, StringComparison.OrdinalIgnoreCase);

            if (sourceProperty is not null && targetProperty is null) {
                if (sourceHasClassification)
                    ops.Add(GenerateRemoveOperation(source));
                return ops;
            }

            if (sourceProperty is null && targetProperty is not null) {
                if (targetHasClassification)
                    ops.Add(GenerateCreateOperation(target, targetProperty));
                return ops;
            }

            if (sourceHasClassification && !targetHasClassification) {
                ops.Add(GenerateRemoveOperation(isRenamed ? source : target));
                return ops;
            }

            if (!sourceHasClassification && targetHasClassification) {
                ops.Add(GenerateCreateOperation(target, targetProperty!));
                return ops;
            }

            if (sourceHasClassification && targetHasClassification) {
                if (isRenamed) {
                    ops.Add(GenerateRemoveOperation(source));
                    ops.Add(GenerateCreateOperation(target, targetProperty!));
                } else if (HasDataClassificationChanged(sourceProperty!, targetProperty!)) {
                    ops.Add(GenerateRemoveOperation(target));
                    ops.Add(GenerateCreateOperation(target, targetProperty!));
                }
            }

            return ops;
        }

        /// <summary>
        /// Ensures classification operations run in correct order: remove before column changes, create after.
        /// </summary>
        protected override IReadOnlyList<MigrationOperation> Sort(IEnumerable<MigrationOperation> operations, DiffContext diffContext) {
            var sorted = base.Sort(operations, diffContext).ToList();

            for (var i = 0; i < sorted.Count; i++) {
                var columnOp = sorted[i];
                
                var (schema, table, oldColumn, newColumn) = columnOp switch {
                    DropColumnOperation drop => (drop.Schema, drop.Table, drop.Name, (string?)null),
                    RenameColumnOperation rename => (rename.Schema, rename.Table, rename.Name, rename.NewName),
                    AlterColumnOperation alter => (alter.Schema, alter.Table, alter.Name, (string?)null),
                    _ => (null, null, null, null)
                };

                if (table == null || oldColumn == null)
                    continue;

                MoveRemoveOperationBefore(sorted, ref i, schema, table, oldColumn);

                var targetColumn = newColumn ?? oldColumn;
                MoveCreateOperationAfter(sorted, i, schema, table, targetColumn);
            }

            return sorted;
        }

        /// <summary>
        /// Moves remove classification operation before column operations (drop/rename/alter).
        /// </summary>
        private static void MoveRemoveOperationBefore(List<MigrationOperation> sorted, ref int targetIdx, string? schema, string table, string column) {
            var removeIdx = sorted.FindIndex(op =>
                op is RemoveDataClassificationOperation remove
                && SchemaEquals(remove.Schema, schema)
                && string.Equals(remove.Table, table, StringComparison.OrdinalIgnoreCase)
                && string.Equals(remove.Column, column, StringComparison.OrdinalIgnoreCase));

            if (removeIdx >= 0 && removeIdx > targetIdx) {
                var remove = sorted[removeIdx];
                sorted.RemoveAt(removeIdx);
                sorted.Insert(targetIdx, remove);
                targetIdx++; 
            }
        }

        /// <summary>
        /// Moves create classification operation after column operations (drop/rename/alter).
        /// </summary>
        private static void MoveCreateOperationAfter(List<MigrationOperation> sorted, int targetIdx, string? schema, string table, string column) {
            var createIdx = sorted.FindIndex(op =>
                op is CreateDataClassificationOperation create
                && SchemaEquals(create.Schema, schema)
                && string.Equals(create.Table, table, StringComparison.OrdinalIgnoreCase)
                && string.Equals(create.Column, column, StringComparison.OrdinalIgnoreCase));

            if (createIdx >= 0 && createIdx <= targetIdx) {
                var create = sorted[createIdx];
                sorted.RemoveAt(createIdx);
                // Insert after the target operation (adjust index if we removed before target)
                var insertIdx = createIdx < targetIdx ? targetIdx : targetIdx + 1;
                sorted.Insert(insertIdx, create);
            }
        }

        /// <summary>
        /// Compares schemas considering null and "dbo" as equivalent.
        /// </summary>
        private static bool SchemaEquals(string? schema1, string? schema2) {
            var s1 = string.IsNullOrEmpty(schema1) ? DataClassificationConstants.DefaultSchema : schema1;
            var s2 = string.IsNullOrEmpty(schema2) ? DataClassificationConstants.DefaultSchema : schema2;
            return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Helpers

        private static void AddCreateOperationIfNeeded(List<MigrationOperation> ops, IColumn column) {
            var prop = GetMappedProperty(column);
            if (prop is not null && HasClassification(prop))
                ops.Add(GenerateCreateOperation(column, prop));
        }

        private static void AddRemoveOperationIfNeeded(List<MigrationOperation> ops, IColumn column) {
            var prop = GetMappedProperty(column);
            if (prop is not null && HasClassification(prop))
                ops.Add(GenerateRemoveOperation(column));
        }

        /// <summary>
        /// Gets the first mapped property for the given column.
        /// </summary>
        /// <param name="column">The column to get the mapped property for.</param>
        /// <returns>The first mapped property, or <c>null</c> if no property is mapped to this column.</returns>
        /// <remarks>
        /// Uses <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/> to return the first mapped property.
        /// In typical EF Core scenarios, a column maps to exactly one property, so this returns the expected property.
        /// </remarks>
        private static IProperty? GetMappedProperty(IColumn column)
            => column.PropertyMappings.FirstOrDefault()?.Property;

        /// <summary>
        /// Checks if a property has any classification metadata (label, informationType, or rank).
        /// </summary>
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
                Rank = GetAnnotation(property, DataClassificationConstants.Rank)
            };

        private static RemoveDataClassificationOperation GenerateRemoveOperation(IColumn column)
            => new() {
                Schema = column.Table.Schema,
                Table = column.Table.Name,
                Column = column.Name
            };

        private static string? GetAnnotation(IProperty property, string key)
            => property.FindAnnotation(key)?.Value?.ToString();


        /// <summary>
        /// Checks if any classification annotation (label, informationType, rank) changed between source and target.
        /// </summary>
        private static bool HasDataClassificationChanged(IProperty sourceProp, IProperty targetProp)
            => HasAnnotationChanged(sourceProp, targetProp, DataClassificationConstants.Label)
               || HasAnnotationChanged(sourceProp, targetProp, DataClassificationConstants.InformationType)
               || HasAnnotationChanged(sourceProp, targetProp, DataClassificationConstants.Rank);

        /// <summary>
        /// Compares annotation values between two properties.
        /// </summary>
        private static bool HasAnnotationChanged(IProperty source, IProperty target, string annotationKey) {
            var sourceValue = source.FindAnnotation(annotationKey)?.Value?.ToString() ?? string.Empty;
            var targetValue = target.FindAnnotation(annotationKey)?.Value?.ToString() ?? string.Empty;
            return !string.Equals(sourceValue, targetValue, StringComparison.Ordinal);
        }

        #endregion
    }
}

#pragma warning restore EF1001
