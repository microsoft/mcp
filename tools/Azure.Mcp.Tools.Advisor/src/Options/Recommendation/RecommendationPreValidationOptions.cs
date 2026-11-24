// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public sealed class RecommendationPreValidationOptions : IRecommendationReadScopeOptions
{
    [Option(Description = "The exact Advisor recommendation type ID GUID selected from existing recommendations. Only onboarded Cost/IdleResources deletion recommendations support prevalidation.")]
    public required string RecommendationTypeId { get; set; }

    [Option(Description = "Selected affected resources, as exact full ARM resource IDs only. Reuse IDs already returned by recommendation list; resolve user-supplied names through list when IDs are unavailable and disambiguate before calling. Omit to validate the selected recommendation type across the scope. Missing selections are reported, not treated as safe.")]
    public string[]? Resources { get; set; }

    [Option(Description = "Optional positive maximum number of affected resources to validate after exact selection. Omit to validate all matching resources. Processing uses bounded batches without imposing a 25-resource total limit. Incomplete discovery or an explicit limit is reported as truncated coverage.")]
    public int? Top { get; set; }

    [Option(Description = "Minimum unused age in days, from 1 through 365. Defaults to 30. Recent activity, missing signals, or insufficient history require review; this check never overrides a confirmed blocker.")]
    public int? MinimumUnusedDays { get; set; }

    [Option(Description = "An Azure Service Group name, ID, or full ARM resource ID. Specify either a single Service Group or subscription scope, not both. Resource groups apply only to subscription scope.")]
    public string? ServiceGroup { get; set; }

    [Option(Description = "One or more subscription IDs or names. Pass a one-element array for one subscription. Cannot be combined with --service-group. When both scopes are omitted, the configured default subscription is used.")]
    public string[]? Subscriptions { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
