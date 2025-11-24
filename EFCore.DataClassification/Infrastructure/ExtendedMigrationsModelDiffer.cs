using System.Collections.Generic;
using System.Linq;
using EFCore.DataClassification.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace EFCore.DataClassification.Infrastructure {
    /// <summary>
    /// DataClassification annotation'larındaki değişiklikleri algılayıp
    /// gerekiyorsa ekstra AlterColumnOperation ekleyen differ.
    /// </summary>
    public sealed class DataClassificationMigrationsModelDiffer : MigrationsModelDiffer {
        public DataClassificationMigrationsModelDiffer(
            IRelationalTypeMappingSource typeMappingSource,
            IMigrationsAnnotationProvider migrationsAnnotationProvider,
            IRowIdentityMapFactory rowIdentityMapFactory,
            CommandBatchPreparerDependencies commandBatchPreparerDependencies)
            : base(typeMappingSource, migrationsAnnotationProvider, rowIdentityMapFactory, commandBatchPreparerDependencies) {
        }

        protected override IEnumerable<MigrationOperation> Diff(
            IColumn source,
            IColumn target,
            DiffContext diffContext) {
            // 1) EF'nin normal ürettiği operasyonlar
            var baseOps = base.Diff(source, target, diffContext).ToList();

            // EF zaten bir AlterColumn üretmişse, biz ekstra bir şey eklemiyoruz.
            if (baseOps.OfType<AlterColumnOperation>().Any()) {
                return baseOps;
            }

            // 2) Sadece DataClassification değişmiş mi?
            if (!HasDataClassificationChanged(source, target)) {
                return baseOps;
            }

            var sourceProperty = source.PropertyMappings.FirstOrDefault()?.Property;
            var targetProperty = target.PropertyMappings.FirstOrDefault()?.Property;
            if (targetProperty is null) {
                return baseOps;
            }

            static Type SafeType(Type? type)
                => type == null || type.Namespace == null ? typeof(string) : type;

            var newClrType = SafeType(targetProperty.ClrType);
            var oldClrType = SafeType(sourceProperty?.ClrType ?? targetProperty.ClrType);

            var alter = new AlterColumnOperation {
                Schema = target.Table.Schema,
                Table = target.Table.Name,
                Name = target.Name,

                // YENİ değerler
                ClrType = newClrType,
                ColumnType = target.StoreType ?? targetProperty.GetColumnType(),
                IsNullable = target.IsNullable,
                MaxLength = target.MaxLength,
                Precision = target.Precision,
                Scale = target.Scale,
                IsUnicode = target.IsUnicode,
                IsRowVersion = target.IsRowVersion,

                // ESKİ değerler (OldColumn property'si içinde)
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

            baseOps.Add(alter);
            return baseOps;
        }

        private static bool HasDataClassificationChanged(IColumn source, IColumn target) {
            var sProp = source.PropertyMappings.FirstOrDefault()?.Property;
            var tProp = target.PropertyMappings.FirstOrDefault()?.Property;

            if (sProp is null || tProp is null)
                return false;

            static string Get(IProperty p, string key)
                => p.FindAnnotation(key)?.Value?.ToString() ?? string.Empty;

            var sLabel = Get(sProp, DataClassificationConstants.Label);
            var tLabel = Get(tProp, DataClassificationConstants.Label);

            var sInfo = Get(sProp, DataClassificationConstants.InformationType);
            var tInfo = Get(tProp, DataClassificationConstants.InformationType);

            var sRank = Get(sProp, DataClassificationConstants.Rank);
            var tRank = Get(tProp, DataClassificationConstants.Rank);

            return sLabel != tLabel || sInfo != tInfo || sRank != tRank;
        }
    }
}