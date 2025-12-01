using System;
using System.Collections.Generic;
using System.Linq;
using EFCore.DataClassification.Annotations;
using EFCore.DataClassification.Operations;
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


        protected override IEnumerable<MigrationOperation> Diff(IColumn source, IColumn target, DiffContext diffContext) {
            var baseOps = base.Diff(source, target, diffContext).ToList();

            

            var sourceProperty = source.PropertyMappings.FirstOrDefault()?.Property;
            var targetProperty = target.PropertyMappings.FirstOrDefault()?.Property;

            if (sourceProperty is null && targetProperty is null)
                return baseOps;

            if (sourceProperty != null && targetProperty is null) {
                baseOps.Add(GenerateRemoveOperation(source));
                return baseOps;
            }

            if (sourceProperty is null && targetProperty != null) {
                baseOps.Add(GenerateCreateOperation(target, targetProperty));
                return baseOps;
            }

            if (!HasDataClassificationChanged(source, target) || targetProperty is null)
                return baseOps;

            baseOps.Add(GenerateRemoveOperation(target));
            baseOps.Add(GenerateCreateOperation(target, targetProperty));
            return baseOps;
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