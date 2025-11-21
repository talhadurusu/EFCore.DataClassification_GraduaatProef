using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EFCore.DataClassification.Models;
using EFCore.DataClassification.Constants;

namespace EFCore.DataClassification.Extensions;

public static class PropertyBuilderExtensions {
    // 1. ORTAK MANTIK (Private Metod)
    // Kod tekrarını önlemek için asıl işi yapan metodu buraya ayırdık.
    // PropertyBuilder<TProperty>, PropertyBuilder'dan türediği için parametre olarak onu kabul eder.
    private static void SetClassificationAnnotations( PropertyBuilder builder,string label,string informationType,
        SensitivityRank rank) {
        builder.HasAnnotation(DataClassificationConstants.Label, label);
        builder.HasAnnotation(DataClassificationConstants.InformationType, informationType);
        builder.HasAnnotation(DataClassificationConstants.Rank, rank);
    }

    // 2. Non-Generic  (builder.Property("Name"))
    public static PropertyBuilder HasDataClassification( this PropertyBuilder propertyBuilder,string label,string informationType,SensitivityRank rank) {

        SetClassificationAnnotations(propertyBuilder, label, informationType, rank);
        return propertyBuilder;
    }

    // 3. Generic  (builder.Property(x => x.Name))
    public static PropertyBuilder<TProperty> HasDataClassification<TProperty>(this PropertyBuilder<TProperty> propertyBuilder,string label,string informationType,SensitivityRank rank) {

        SetClassificationAnnotations(propertyBuilder, label, informationType, rank);
        return propertyBuilder;
    }
}