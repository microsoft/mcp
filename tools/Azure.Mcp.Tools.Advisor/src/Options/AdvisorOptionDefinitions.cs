// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Options;

public static class AdvisorOptionDefinitions
{
    public const string CategoryText = "category";
    public const string ImpactText = "impact";
    public const string ResourceTypeText = "resource-type";
    public const string ResourceText = "resource";
    public const string SearchText = "search";
    public const string GroupByText = "group-by";
    public const string TopText = "top";

    public static readonly Option<string> Category = new(
        $"--{CategoryText}"
    )
    {
        Description = "Filter recommendations by category (e.g., 'Security', 'Cost', 'Performance', 'HighAvailability', 'OperationalExcellence'). Case-insensitive exact match.",
        Required = false
    };

    public static readonly Option<string> Impact = new(
        $"--{ImpactText}"
    )
    {
        Description = "Filter recommendations by business impact ('High', 'Medium', or 'Low'). Case-insensitive exact match.",
        Required = false
    };

    public static readonly Option<string> ResourceType = new(
        $"--{ResourceTypeText}"
    )
    {
        Description = "Filter recommendations by impacted Azure resource type (e.g., 'Microsoft.Storage/storageAccounts'). Case-insensitive exact match.",
        Required = false
    };

    public static readonly Option<string> Resource = new(
        $"--{ResourceText}"
    )
    {
        Description = "Filter recommendations by impacted resource name or full ARM resource ID. Case-insensitive substring match.",
        Required = false
    };

    public static readonly Option<string> Search = new(
        $"--{SearchText}"
    )
    {
        Description = "Filter recommendations whose problem text contains this string. Case-insensitive substring match.",
        Required = false
    };

    public static readonly Option<string> GroupBy = new(
        $"--{GroupByText}"
    )
    {
        Description = "Field to group the summary by. One of: 'recommendation', 'category', 'impact', 'resource-type', 'resource'.",
        Required = true
    };

    public static readonly Option<int> Top = new(
        $"--{TopText}"
    )
    {
        Description = "Maximum number of groups to return, ordered by descending count. Defaults to 5; clamped to 1-50.",
        Required = false,
        DefaultValueFactory = _ => 5
    };
}
