using EFCore.DataClassification.Attributes;
using EFCore.DataClassification.Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EFCore.DataClassification.Extensions {
    public static class ModelBuilderExtensions {
        /// <summary>
        /// Scans all entities in the project and finds properties with the [DataClassification] attribute.
        /// Saves the found data as EF Core Metadata (Annotation).
        /// </summary>
        public static void UseDataClassification(this ModelBuilder modelBuilder) {
            // 1. Modeldeki tüm Entity'leri (Tabloları) gez
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
                // 2. Entity'nin C# tipini (CLR Type) al
                var clrType = entityType.ClrType;
                if (clrType == null) continue;

                // 3. O Entity'deki tüm Property'leri (Kolonları) gez
                foreach (var property in entityType.GetProperties()) {
                    // EF Core property'sinin C# karşılığını bul
                    var memberInfo = property.PropertyInfo;
                    if (memberInfo == null) continue;

                    // 4. [DataClassification] attribute'u var mı diye bak
                    var attribute = memberInfo.GetCustomAttribute<DataClassificationAttribute>();

                    if (attribute != null) {
                        // 5. Varsa, bu bilgileri EF Core Metadata'sına (Annotation) işle
                        property.AddAnnotation(DataClassificationConstants.Label, attribute.Label);
                        property.AddAnnotation(DataClassificationConstants.InformationType, attribute.InformationType);
                        property.AddAnnotation(DataClassificationConstants.Rank, attribute.Rank);
                    }
                }
            }
        }
    }
}
