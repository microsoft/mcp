// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Models;

namespace Azure.Mcp.Tools.Workbooks.Options.Workbook;

public class ListWorkbooksOptions : GlobalOptions
{
    public string[]? Subscriptions { get; set; }
    public string[]? ResourceGroups { get; set; }
    public string? Kind { get; set; }
    public string? Category { get; set; }
    public string? SourceId { get; set; }
    public string? NameContains { get; set; }
    public DateTimeOffset? ModifiedAfter { get; set; }
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Standard;
    public int MaxResults { get; set; } = 50;
    public bool IncludeTotalCount { get; set; } = true;

    /// <summary>
    /// Creates a WorkbookFilters object from the command options.
    /// </summary>
    public WorkbookFilters ToFilters() => new()
    {
        Kind = Kind,
        Category = Category,
        SourceId = SourceId,
        NameContains = NameContains,
        ModifiedAfter = ModifiedAfter
    };
}
