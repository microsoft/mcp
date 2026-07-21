// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Workbooks.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Workbooks.Options.Workbook;

public sealed class ListWorkbooksOptions : ISubscriptionOption
{
    [Option(Description = "Filter workbooks by kind (e.g., 'shared', 'user'). If not specified, all kinds will be returned.")]
    public string? Kind { get; set; }

    [Option(Description = "Filter workbooks by category (e.g., 'workbook', 'sentinel', 'TSG'). If not specified, all categories will be returned.")]
    public string? Category { get; set; }

    [Option(Description = "Filter workbooks by source resource ID (e.g., Application Insights resource, Log Analytics workspace). If not specified, all workbooks will be returned.")]
    public string? SourceId { get; set; }

    [Option(Description = "Filter workbooks where display name contains this text (case-insensitive).")]
    public string? NameContains { get; set; }

    [Option(Description = "Filter workbooks modified after this date (ISO 8601 format, e.g., '2024-01-15').")]
    public string? ModifiedAfter { get; set; }
    internal DateTimeOffset? ParsedModifiedAfter { get; set; }

    [Option(Description = "Output format: 'summary' (id+name only, minimal tokens), 'standard' (metadata without content, default), 'full' (includes serializedData).")]
    public string? OutputFormat { get; set; }

    [Option(Description = "Maximum number of results to return (default: 50, max: 1000).")]
    public int? MaxResults { get; set; }

    [Option(Description = "Include total count of all matching workbooks in the response (default: true).")]
    public bool? IncludeTotalCount { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    /// <summary>
    /// Creates a WorkbookFilters object from the command options.
    /// </summary>
    public WorkbookFilters ToFilters() => new()
    {
        Kind = Kind,
        Category = Category,
        SourceId = SourceId,
        NameContains = NameContains,
        ModifiedAfter = ParsedModifiedAfter
    };
}
