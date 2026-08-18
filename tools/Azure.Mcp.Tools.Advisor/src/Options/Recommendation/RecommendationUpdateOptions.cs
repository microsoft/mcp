// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public sealed class RecommendationUpdateOptions : ISubscriptionOption
{
    [Option(Description = "The stable ID of the Advisor recommendation to update. The REST API and this command also call this value the recommendation ID.")]
    public required string RecommendationId { get; set; }

    [Option(Description = "The new recommendation state: New, Postponed, Dismissed, or Completed. Use New to reactivate a postponed or dismissed recommendation.")]
    public required RecommendationStatus RecommendationStatus { get; set; }

    [Option(Description = "The date and time until which the recommendation is postponed, in ISO 8601 format with a timezone offset. Required when --recommendation-status is Postponed and must represent a future instant.")]
    public DateTimeOffset? PostponedUntilDateTime { get; set; }

    [Option(Description = "The explicit reason for dismissing the recommendation. Required when --recommendation-status is Dismissed. " +
        "Map natural-language intent as follows: cost investment is too high to ExcessiveCostInvestmentRequired; unclear steps to ImplementationStepsAreUnclear; " +
        "incompatible configuration to IncompatibleWithTheCurrentConfiguration; acceptable risk to RiskIsAcceptable; too complex or impractical to TooComplexOrImpracticalToImplement; " +
        "an existing alternative to AnAlternativeSolutionIsAlreadyInPlace; and a clearly stated reason outside these choices to Other. " +
        "If the user only asks to dismiss without giving a reason, ask them to choose a reason instead of defaulting to Other.")]
    public RecommendationDismissReason? RecommendationDismissReason { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
