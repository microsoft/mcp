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
        Description = "Free-text filter applied to the recommendation problem text (case-insensitive substring match). " +
            "Use this whenever the user's request includes a topical phrase such as 'related to Microsoft Foundry', " +
            "'about encryption', 'mentioning right-size', or 'for Key Vault'. " +
            "Extract the salient noun(s) from the phrase (e.g., 'Foundry', 'encrypt', 'right-size', 'Key Vault') and pass them here.",
        Required = false
    };

    public static readonly Option<string> GroupBy = new(
        $"--{GroupByText}"
    )
    {
        Description = "Optional field to group the summary by. One of: 'recommendation-type', 'category', 'impact', 'resource-type'. " +
            "Defaults to 'category' when omitted, which surfaces the high-level themes (Cost, Security, Reliability, etc.) " +
            "so prompts like 'summarize the key themes from my Advisor recommendations' work without naming a field.",
        Required = false
    };

    public static readonly Option<int> Top = new(
        $"--{TopText}"
    )
    {
        Description = "Maximum number of items to return. " +
            "For 'list': defaults to 50, clamped to 1-100 (server-side limit). " +
            "For 'summary': optional display cap on the number of buckets returned (defaults to all). " +
            "TotalRecommendations always reflects the complete filtered population regardless of --top.",
        Required = false
    };
}
