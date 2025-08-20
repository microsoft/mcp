// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Workbooks.Models;

/// <summary>
/// Represents filtering options for workbook queries.
/// </summary>
public sealed record WorkbookFilters
{
    /// <summary>
    /// Filter workbooks by kind (e.g., 'shared', 'user').
    /// </summary>
    public string? Kind { get; init; }

    /// <summary>
    /// Filter workbooks by category (e.g., 'workbook', 'sentinel', 'TSG').
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Filter workbooks by source resource ID (e.g., Application Insights resource, Log Analytics workspace).
    /// </summary>
    public string? SourceId { get; init; }

    /// <summary>
    /// Creates an empty filter (no filtering applied).
    /// </summary>
    public static WorkbookFilters Empty => new();

    /// <summary>
    /// Checks if any filters are applied.
    /// </summary>
    public bool HasFilters => !string.IsNullOrEmpty(Kind) || !string.IsNullOrEmpty(Category) || !string.IsNullOrEmpty(SourceId);
}
