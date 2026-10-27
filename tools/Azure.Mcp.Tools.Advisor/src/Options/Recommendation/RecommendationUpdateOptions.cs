// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public sealed class RecommendationUpdateOptions : IRecommendationScopeOptions
{
    [Option(Description = "The name, ID, or full ARM resource ID of the Azure Service Group that contains the recommendation. The tool resolves the scope in the selected tenant and reports ambiguous names. Specify either --service-group or --subscription. If both are omitted, the configured default subscription is used.")]
    public string? ServiceGroup { get; set; }

    [Option(Description = "The stable ID of the Advisor recommendation to update. The REST API and this command also call this value the recommendation ID.")]
    public string? RecommendationId { get; set; }

    [Option(Description = "The explicit stable recommendation IDs to update together in one destructive tool call. All IDs share the subscription or Service Group, tenant, status, and date or reason. Specify either this batch or --recommendation-id, not both. Each recommendation is updated independently and returns its own outcome; the batch is not atomic.")]
    public string[]? RecommendationIds { get; set; }

    [Option(Description = "The new recommendation state: New, Postponed, Dismissed, or Completed. Use New to reactivate a postponed, dismissed, or completed recommendation.")]
    public required RecommendationStatus RecommendationStatus { get; set; }

    [Option(Description = "The date and time until which the recommendation is postponed, in ISO 8601 format with a timezone offset. Required when --recommendation-status is Postponed and must represent a future instant.")]
    public string? PostponedUntilDateTime { get; set; }

    [Option(Description = "The reason for dismissing the recommendation. " +
        "Map natural-language intent as follows: cost investment is too high to ExcessiveCostInvestmentRequired; unclear steps to ImplementationStepsAreUnclear; " +
        "incompatible configuration to IncompatibleWithTheCurrentConfiguration; acceptable risk to RiskIsAcceptable; too complex or impractical to TooComplexOrImpracticalToImplement; " +
        "an existing alternative to AnAlternativeSolutionIsAlreadyInPlace; and a clearly stated reason outside these choices to Other. " +
        "When dismissing, omit this option or use Other if no reason was supplied or the natural-language intent cannot be mapped to a supported value; an omitted reason defaults to Other.")]
    public RecommendationDismissReason? RecommendationDismissReason { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
