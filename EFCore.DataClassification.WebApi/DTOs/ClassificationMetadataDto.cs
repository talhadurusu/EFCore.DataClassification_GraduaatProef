namespace EFCore.DataClassification.WebApi.DTOs;

/// <summary>
/// Represents classification metadata for a database column.
/// Used for compliance reporting (NIS2/GDPR).
/// </summary>
public class ClassificationMetadataDto
{
    /// <summary>
    /// Database schema name (e.g., "dbo").
    /// </summary>
    public string Schema { get; set; } = "dbo";

    /// <summary>
    /// Database table name.
    /// </summary>
    public string Table { get; set; } = null!;

    /// <summary>
    /// Database column name.
    /// </summary>
    public string Column { get; set; } = null!;

    /// <summary>
    /// Sensitivity label (e.g., "Confidential", "Public", "Internal").
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// Information type (e.g., "Email Address", "Phone Number", "Financial").
    /// </summary>
    public string? InformationType { get; set; }

    /// <summary>
    /// Sensitivity rank (None, Low, Medium, High, Critical).
    /// </summary>
    public string? Rank { get; set; }
}

/// <summary>
/// Summary statistics for data classifications.
/// </summary>
public class ClassificationSummaryDto
{
    /// <summary>
    /// Total number of classified columns.
    /// </summary>
    public int TotalClassifiedColumns { get; set; }

    /// <summary>
    /// Classification counts grouped by sensitivity rank.
    /// </summary>
    public IEnumerable<RankCountDto> ByRank { get; set; } = [];

    /// <summary>
    /// Classification counts grouped by information type.
    /// </summary>
    public IEnumerable<InfoTypeCountDto> ByInformationType { get; set; } = [];

    /// <summary>
    /// Timestamp when the summary was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class RankCountDto
{
    public string? Rank { get; set; }
    public int Count { get; set; }
}

public class InfoTypeCountDto
{
    public string? InformationType { get; set; }
    public int Count { get; set; }
}


