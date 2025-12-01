using EFCore.DataClassification.Annotations;
using EFCore.DataClassification.Attributes;
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
            
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
              
                var clrType = entityType.ClrType;
                if (clrType == null) continue;

                
                foreach (var property in entityType.GetProperties()) {
                   
                    var memberInfo = property.PropertyInfo;
                    if (memberInfo == null) continue;


                    var attribute = memberInfo.GetCustomAttribute<DataClassificationAttribute>();

                    if (attribute != null) {
                       
                        property.SetAnnotation(DataClassificationConstants.Label, attribute.Label);
                        property.SetAnnotation(DataClassificationConstants.InformationType, attribute.InformationType);
                        property.SetAnnotation(DataClassificationConstants.Rank, attribute.Rank);
                    }
                }
            }
        }
    }
}
