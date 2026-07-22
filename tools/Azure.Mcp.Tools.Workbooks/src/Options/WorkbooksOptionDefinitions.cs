// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Workbooks.Options;

public static class WorkbooksOptionDefinitions
{
    public const string WorkbookIdText = "workbook-id";
    public const string WorkbookIdsText = "workbook-ids";
    public const string DisplayNameText = "display-name";
    public const string SerializedContentText = "serialized-content";
    public const string SourceIdText = "source-id";
    public const string KindText = "kind";
    public const string CategoryText = "category";
    public const string NameContainsText = "name-contains";
    public const string ModifiedAfterText = "modified-after";
    public const string OutputFormatText = "output-format";
    public const string MaxResultsText = "max-results";
    public const string IncludeTotalCountText = "include-total-count";

    public static readonly Option<string> WorkbookId = new(
        $"--{WorkbookIdText}"
    )
    {
        Description = "The Azure Resource ID of the workbook to retrieve.",
        Required = true
    };

    public static readonly Option<string[]> WorkbookIds = new(
        $"--{WorkbookIdsText}"
    )
    {
        Description = "The Azure Resource IDs of the workbooks to operate on (supports multiple values for batch operations).",
        Required = true,
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<string> DisplayName = new(
        $"--{DisplayNameText}"
    )
    {
        Description = "The display name of the workbook.",
        Required = false
    };

    public static readonly Option<string> SerializedContent = new(
        $"--{SerializedContentText}"
    )
    {
        Description = "The JSON serialized content/data of the workbook.",
        Required = false
    };

    public static readonly Option<string> SourceId = new(
        $"--{SourceIdText}"
    )
    {
        Description = "The linked resource ID for the workbook. By default, this is 'azure monitor'.",
        Required = false,
    };

    // Command-specific variations for required fields
    public static readonly Option<string> DisplayNameRequired = new(
        $"--{DisplayNameText}"
    )
    {
        Description = "The display name of the workbook.",
        Required = true
    };

    public static readonly Option<string> SerializedContentRequired = new(
        $"--{SerializedContentText}"
    )
    {
        Description = "The serialized JSON content of the workbook.",
        Required = true
    };

    // Filter options for listing workbooks
    public static readonly Option<string> Kind = new(
        $"--{KindText}"
    )
    {
        Description = "Filter workbooks by kind (e.g., 'shared', 'user'). If not specified, all kinds will be returned.",
        Required = false
    };

    public static readonly Option<string> Category = new(
        $"--{CategoryText}"
    )
    {
        Description = "Filter workbooks by category (e.g., 'workbook', 'sentinel', 'TSG'). If not specified, all categories will be returned.",
        Required = false
    };

    public static readonly Option<string> SourceIdFilter = new(
        $"--{SourceIdText}"
    )
    {
        Description = "Filter workbooks by source resource ID (e.g., Application Insights resource, Log Analytics workspace). If not specified, all workbooks will be returned.",
        Required = false
    };

    public static readonly Option<string> NameContains = new(
        $"--{NameContainsText}"
    )
    {
        Description = "Filter workbooks where display name contains this text (case-insensitive).",
        Required = false
    };

    public static readonly Option<string> ModifiedAfter = new(
        $"--{ModifiedAfterText}"
    )
    {
        Description = "Filter workbooks modified after this date (ISO 8601 format, e.g., '2024-01-15').",
        Required = false
    };

    public static readonly Option<string> OutputFormat = new(
        $"--{OutputFormatText}"
    )
    {
        Description = "Output format: 'summary' (id+name only, minimal tokens), 'standard' (metadata without content, default), 'full' (includes serializedData).",
        Required = false
    };

    public static readonly Option<int> MaxResults = new(
        $"--{MaxResultsText}"
    )
    {
        Description = "Maximum number of results to return (default: 50, max: 1000).",
        Required = false
    };

    public static readonly Option<bool> IncludeTotalCount = new(
        $"--{IncludeTotalCountText}"
    )
    {
        Description = "Include total count of all matching workbooks in the response (default: true).",
        Required = false
    };
}
