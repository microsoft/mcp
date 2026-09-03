// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public sealed class RecommendationChaosReviewOptions : ISubscriptionOption
{
    [Option(Description = "The exact active Azure Advisor recommendation type ID GUID to verify.")]
    public required string RecommendationTypeId { get; set; }

    [Option(Description = "The exact ARM resource ID of the Microsoft.Compute/virtualMachineScaleSets resource affected by the active Advisor recommendation.")]
    public required string Resource { get; set; }

    [Option(Description = "Optional exact Microsoft.Chaos workspace ARM ID selected from a prior review result.")]
    public string? Workspace { get; set; }

    [Option(Description = "Optional exact Microsoft.Chaos scenario ARM ID selected from a prior review result.")]
    public string? Scenario { get; set; }

    [Option(Description = "Optional exact Microsoft.Chaos scenario configuration ARM ID selected from a prior review result.")]
    public string? Configuration { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
