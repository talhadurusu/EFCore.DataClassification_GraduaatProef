using EFCore.DataClassification.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace EFCore.DataClassification.WebApi.Controllers;

/// <summary>
/// Controller for managing data classification metadata and reporting.
/// Supports JSON/CSV export for NIS2/GDPR compliance audits.
/// 
/// Exception handling: GlobalExceptionHandler automatically catches all errors
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DataClassificationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataClassificationController> _logger;

    public DataClassificationController(AppDbContext context, ILogger<DataClassificationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // CLASSIFICATION METADATA QUERIES (Reporting)


    /// <summary>
    /// Gets all data classification metadata from the database.
    /// Queries SQL Server's sys.sensitivity_classifications system view.
    /// </summary>
    /// <returns>List of all classified columns with their sensitivity information.</returns>
    [HttpGet("metadata")]
    [ProducesResponseType(typeof(IEnumerable<ClassificationMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllClassifications()
    {
        var classifications = await GetClassificationMetadataAsync();
        return Ok(classifications);
    }

    /// <summary>
    /// Exports all classification metadata as CSV file.
    /// Useful for compliance audits and documentation.
    /// </summary>
    [HttpGet("metadata/export/csv")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportAsCsv() {
        var classifications = await GetClassificationMetadataAsync();

        // Okunabilirlik için sıralama (isteğe bağlı)
        var ordered = classifications
            .OrderBy(x => x.Schema)
            .ThenBy(x => x.Table)
            .ThenBy(x => x.Column);

        const string delimiter = ";";

        var sb = new StringBuilder();
        sb.Append("Schema").Append(delimiter)
          .Append("Table").Append(delimiter)
          .Append("Column").Append(delimiter)
          .Append("Label").Append(delimiter)
          .Append("InformationType").Append(delimiter)
          .Append("Rank").Append("\r\n");

        foreach (var c in ordered) {
            sb.Append(EscapeCsv(c.Schema, delimiter)).Append(delimiter)
              .Append(EscapeCsv(c.Table, delimiter)).Append(delimiter)
              .Append(EscapeCsv(c.Column, delimiter)).Append(delimiter)
              .Append(EscapeCsv(c.Label, delimiter)).Append(delimiter)
              .Append(EscapeCsv(c.InformationType, delimiter)).Append(delimiter)
              .Append(EscapeCsv(c.Rank, delimiter)).Append("\r\n");
        }

        var fileName = $"data-classifications-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        
        var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        return File(utf8Bom.GetBytes(sb.ToString()), "text/csv; charset=utf-8", fileName);
    }

   


    /// <summary>
    /// Exports all classification metadata as JSON file.
    /// </summary>
    [HttpGet("metadata/export/json")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExportAsJson()
    {
        var classifications = await GetClassificationMetadataAsync();

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(classifications, options);

        var fileName = $"data-classifications-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        return File(Encoding.UTF8.GetBytes(json), "application/json", fileName);
    }

    /// <summary>
    /// Filters classifications by sensitivity rank.
    /// </summary>
    /// <param name="rank">Sensitivity rank: None, Low, Medium, High, Critical</param>
    [HttpGet("metadata/by-rank/{rank}")]
    [ProducesResponseType(typeof(IEnumerable<ClassificationMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByRank(string rank)
    {
        var classifications = await GetClassificationMetadataAsync();
        var filtered = classifications.Where(c =>
            c.Rank != null && c.Rank.Equals(rank, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered);
    }

    /// <summary>
    /// Filters classifications by information type.
    /// </summary>
    /// <param name="infoType">Information type (e.g., "Email Address", "Phone Number")</param>
    [HttpGet("metadata/by-info-type/{infoType}")]
    [ProducesResponseType(typeof(IEnumerable<ClassificationMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByInformationType(string infoType)
    {
        var classifications = await GetClassificationMetadataAsync();
        var filtered = classifications.Where(c =>
            c.InformationType != null && c.InformationType.Contains(infoType, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered);
    }

    /// <summary>
    /// Gets classification summary statistics.
    /// Useful for dashboards and compliance overview.
    /// </summary>
    [HttpGet("metadata/summary")]
    [ProducesResponseType(typeof(ClassificationSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSummary()
    {
        var classifications = (await GetClassificationMetadataAsync()).ToList();

        var summary = new ClassificationSummaryDto
        {
            TotalClassifiedColumns = classifications.Count,
            ByRank = classifications
                .GroupBy(c => c.Rank ?? "Unknown")
                .Select(g => new RankCountDto { Rank = g.Key, Count = g.Count() })
                .OrderByDescending(r => r.Count),
            ByInformationType = classifications
                .GroupBy(c => c.InformationType ?? "Unknown")
                .Select(g => new InfoTypeCountDto { InformationType = g.Key, Count = g.Count() })
                .OrderByDescending(i => i.Count),
            GeneratedAt = DateTime.UtcNow
        };

        return Ok(summary);
    }

    /// <summary>
    /// Gets classifications for a specific table.
    /// </summary>
    /// <param name="tableName">Name of the database table</param>
    [HttpGet("metadata/by-table/{tableName}")]
    [ProducesResponseType(typeof(IEnumerable<ClassificationMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByTable(string tableName)
    {
        var classifications = await GetClassificationMetadataAsync();
        var filtered = classifications.Where(c =>
            c.Table.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        return Ok(filtered);
    }

    // EXTENDED PROPERTIES QUERIES


    /// <summary>
    /// Gets all extended properties (alternative classification storage).
    /// Extended properties are used as fallback when sensitivity classifications are not available.
    /// </summary>
    [HttpGet("extended-properties")]
    [ProducesResponseType(typeof(IEnumerable<ClassificationMetadataDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExtendedProperties()
    {
        var properties = await GetExtendedPropertiesAsync();
        return Ok(properties);
    }


    // PRIVATE HELPERS


    /// <summary>
    /// Queries SQL Server for sensitivity classification metadata.
    /// Uses sys.sensitivity_classifications system view (SQL Server 2019+).
    /// </summary>
    private async Task<IEnumerable<ClassificationMetadataDto>> GetClassificationMetadataAsync()
    {
        const string sql = @"
            SELECT 
                SCHEMA_NAME(o.schema_id) AS SchemaName,
                o.name AS TableName,
                c.name AS ColumnName,
                sc.label AS Label,
                sc.information_type AS InformationType,
                sc.rank_desc AS Rank
            FROM sys.sensitivity_classifications sc
            JOIN sys.objects o ON sc.major_id = o.object_id
            JOIN sys.columns c ON sc.major_id = c.object_id 
                AND sc.minor_id = c.column_id
            ORDER BY o.name, c.name";

        var results = new List<ClassificationMetadataDto>();

        await using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new ClassificationMetadataDto
            {
                Schema = reader.GetString(0),
                Table = reader.GetString(1),
                Column = reader.GetString(2),
                Label = reader.IsDBNull(3) ? null : reader.GetString(3),
                InformationType = reader.IsDBNull(4) ? null : reader.GetString(4),
                Rank = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return results;
    }

    /// <summary>
    /// Queries SQL Server for extended properties containing classification data.
    /// </summary>
    private async Task<IEnumerable<ClassificationMetadataDto>> GetExtendedPropertiesAsync()
    {
        const string sql = @"
            SELECT 
                SCHEMA_NAME(t.schema_id) AS SchemaName,
                t.name AS TableName,
                c.name AS ColumnName,
                MAX(CASE WHEN ep.name = 'DataClassification_Label' THEN CAST(ep.value AS NVARCHAR(MAX)) END) AS Label,
                MAX(CASE WHEN ep.name = 'DataClassification_InformationType' THEN CAST(ep.value AS NVARCHAR(MAX)) END) AS InformationType,
                MAX(CASE WHEN ep.name = 'DataClassification_Rank' THEN CAST(ep.value AS NVARCHAR(MAX)) END) AS Rank
            FROM sys.extended_properties ep
            JOIN sys.tables t ON ep.major_id = t.object_id
            JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id
            WHERE ep.name LIKE 'DataClassification_%'
            GROUP BY t.schema_id, t.name, c.name
            ORDER BY t.name, c.name";

        var results = new List<ClassificationMetadataDto>();

        await using var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new ClassificationMetadataDto
            {
                Schema = reader.GetString(0),
                Table = reader.GetString(1),
                Column = reader.GetString(2),
                Label = reader.IsDBNull(3) ? null : reader.GetString(3),
                InformationType = reader.IsDBNull(4) ? null : reader.GetString(4),
                Rank = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return results;
    }

    /// <summary>
    /// Escapes a value for CSV format.
    /// </summary>
    private static string EscapeCsv(string? value, string delimiter) {
        value ??= "";
        var mustQuote =
            value.Contains(delimiter) ||
            value.Contains('"') ||
            value.Contains('\r') ||
            value.Contains('\n');

        if (!mustQuote) return value;

        // CSV standard: " => ""
        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }
}



