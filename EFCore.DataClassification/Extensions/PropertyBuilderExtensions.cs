using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EFCore.DataClassification.Models;
using EFCore.DataClassification.Annotations;

namespace EFCore.DataClassification.Extensions;

public static class PropertyBuilderExtensions {
   
    private static void SetClassificationAnnotations( PropertyBuilder builder,string label,string informationType,
        SensitivityRank rank) {
        builder.HasAnnotation(DataClassificationConstants.Label, label);
        builder.HasAnnotation(DataClassificationConstants.InformationType, informationType);
        builder.HasAnnotation(DataClassificationConstants.Rank, rank);
    }

    public static PropertyBuilder HasDataClassification( this PropertyBuilder propertyBuilder,string label,string informationType,SensitivityRank rank) {
        SetClassificationAnnotations(propertyBuilder, label, informationType, rank);
        return propertyBuilder;
    }

    public static PropertyBuilder<TProperty> HasDataClassification<TProperty>(this PropertyBuilder<TProperty> propertyBuilder,string label,string informationType,SensitivityRank rank) {

        SetClassificationAnnotations(propertyBuilder, label, informationType, rank);
        return propertyBuilder;
    }
}