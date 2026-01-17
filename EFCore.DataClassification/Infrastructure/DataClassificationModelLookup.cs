using System;
using EFCore.DataClassification.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.DataClassification.Infrastructure;

public static class DataClassificationModelLookup {
    public static bool TryGetTriplet(
        IRelationalModel model,
        string? schema,
        string table,
        string column,
        out string label,
        out string informationType,
        out string rank) {
        label = informationType = rank = string.Empty;
        schema ??= "dbo";

        var t = model.FindTable(table, schema);
        if (t is null && string.Equals(schema, "dbo", StringComparison.OrdinalIgnoreCase)) {
            t = model.FindTable(table, null);
        } else if (t is null && string.IsNullOrWhiteSpace(schema)) {
            t = model.FindTable(table, "dbo");
        }

        if (t is null) return false;

        var c = t.FindColumn(column);
        if (c is null) return false;

        foreach (var mapping in c.PropertyMappings) {
            var prop = mapping.Property;
            var l = prop.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString();
            var i = prop.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString();
            var r = prop.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString();

            if (string.IsNullOrWhiteSpace(l) &&
                string.IsNullOrWhiteSpace(i) &&
                string.IsNullOrWhiteSpace(r))
                continue;

            label = l ?? "";
            informationType = i ?? "";
            rank = r ?? "";
            return true;
        }

        return false;
    }
}
