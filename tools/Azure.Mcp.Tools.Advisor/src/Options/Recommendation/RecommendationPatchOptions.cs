// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public sealed class RecommendationPatchOptions : ISubscriptionOption
{
    [Option(Description = "The stable ID of the Advisor recommendation to update, also called the recommendation ID.")]
    public required string RecommendationId { get; set; }

    [Option(Description = "The new recommendation state: New, Postponed, Dismissed, or Completed. Use New to reactivate a postponed or dismissed recommendation.")]
    public required RecommendationStatus RecommendationStatus { get; set; }

    [Option(Description = "The UTC date and time until which the recommendation is postponed, in ISO 8601 format. Required when --recommendation-status is Postponed and must be in the future.")]
    public DateTimeOffset? PostponedUntilDateTime { get; set; }

    [Option(Description = "The reason for dismissing the recommendation. Required when --recommendation-status is Dismissed. Allowed values: ExcessiveCostInvestmentRequired, ImplementationStepsAreUnclear, IncompatibleWithTheCurrentConfiguration, RiskIsAcceptable, TooComplexOrImpracticalToImplement, AnAlternativeSolutionIsAlreadyInPlace, Other.")]
    public RecommendationDismissReason? RecommendationDismissReason { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
