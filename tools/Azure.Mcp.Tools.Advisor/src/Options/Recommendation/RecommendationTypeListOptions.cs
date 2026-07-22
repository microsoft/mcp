// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public sealed class RecommendationTypeListOptions
{
    [Option(Description = "Optional Azure resource type to narrow results to (e.g. 'microsoft.compute/virtualmachines', 'microsoft.sql/servers'). Matched case-insensitively against the `supportedResourceType` field on each recommendation type. Use this when onboarding a new resource type to see only the recommendations Advisor will generate for it.")]
    public string? ResourceType { get; set; }

    [Option(Description = "Optional impact level filter. Allowed values: 'High', 'Medium', 'Low' (case-insensitive). When omitted, results contain all impact levels but are still sorted High → Medium → Low.")]
    public string? Impact { get; set; }

    [Option(Description = "Optional Advisor category filter. Typical values: 'Cost', 'HighAvailability', 'Security', 'Performance', 'OperationalExcellence' (case-insensitive). New categories to be supported by Advisor in the future will still match.")]
    public string? Category { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
