// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public static class RecommendationTypeListOptionDefinitions
{
    public const string ResourceTypeText = "resource-type";
    public const string ImpactText = "impact";
    public const string CategoryText = "category";

    public static readonly Option<string> ResourceType = new(
        $"--{ResourceTypeText}"
    )
    {
        Description = "Optional Azure resource type to narrow results to (e.g. 'microsoft.compute/virtualmachines', 'microsoft.sql/servers'). Matched case-insensitively against the `supportedResourceType` field on each recommendation type. Use this when onboarding a new resource type to see only the recommendations Advisor will generate for it.",
        Required = false
    };

    public static readonly Option<string> Impact = new(
        $"--{ImpactText}"
    )
    {
        Description = "Optional impact level filter. Allowed values: 'High', 'Medium', 'Low' (case-insensitive). When omitted, results contain all impact levels but are still sorted High → Medium → Low.",
        Required = false
    };

    public static readonly Option<string> Category = new(
        $"--{CategoryText}"
    )
    {
        Description = "Optional Advisor category filter. Typical values: 'Cost', 'HighAvailability', 'Security', 'Performance', 'OperationalExcellence' (case-insensitive). New categories to be supported by Advisor in the future will still match.",
        Required = false
    };
}
