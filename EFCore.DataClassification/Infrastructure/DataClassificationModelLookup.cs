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
        if (t is null) return false;

        var c = t.FindColumn(column);
        if (c is null) return false;

        var l = c.FindAnnotation(DataClassificationConstants.Label)?.Value?.ToString();
        var i = c.FindAnnotation(DataClassificationConstants.InformationType)?.Value?.ToString();
        var r = c.FindAnnotation(DataClassificationConstants.Rank)?.Value?.ToString();

        if (string.IsNullOrWhiteSpace(l) &&
            string.IsNullOrWhiteSpace(i) &&
            string.IsNullOrWhiteSpace(r))
            return false;

        label = l ?? "";
        informationType = i ?? "";
        rank = r ?? "";
        return true;
    }
}
