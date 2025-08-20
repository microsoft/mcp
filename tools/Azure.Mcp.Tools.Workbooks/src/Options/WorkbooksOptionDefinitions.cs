// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Workbooks.Options;

public static class WorkbooksOptionDefinitions
{
    public const string WorkbookIdText = "workbook-id";
    public const string DisplayNameText = "display-name";
    public const string SerializedContentText = "serialized-content";
    public const string SourceIdText = "source-id";
    public const string KindText = "kind";
    public const string CategoryText = "category";

    public static readonly Option<string> WorkbookId = new(
        $"--{WorkbookIdText}",
        "The Azure Resource ID of the workbook to retrieve."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> DisplayName = new(
        $"--{DisplayNameText}",
        "The display name of the workbook."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string> SerializedContent = new(
        $"--{SerializedContentText}",
        "The JSON serialized content/data of the workbook."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string> SourceId = new(
        $"--{SourceIdText}",
        "The linked resource ID for the workbook. By default, this is 'azure monitor'."
    )
    {
        IsRequired = false,
    };

    // Command-specific variations for required fields
    public static readonly Option<string> DisplayNameRequired = new(
        $"--{DisplayNameText}",
        "The display name of the workbook.")
    {
        IsRequired = true
    };

    public static readonly Option<string> SerializedContentRequired = new(
        $"--{SerializedContentText}",
        "The serialized JSON content of the workbook.")
    {
        IsRequired = true
    };

    // Filter options for listing workbooks
    public static readonly Option<string> Kind = new(
        $"--{KindText}",
        "Filter workbooks by kind (e.g., 'shared', 'user'). If not specified, all kinds will be returned."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string> Category = new(
        $"--{CategoryText}",
        "Filter workbooks by category (e.g., 'workbook', 'sentinel', 'TSG'). If not specified, all categories will be returned."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string> SourceIdFilter = new(
        $"--{SourceIdText}",
        "Filter workbooks by source resource ID (e.g., Application Insights resource, Log Analytics workspace). If not specified, all workbooks will be returned."
    )
    {
        IsRequired = false
    };
}
