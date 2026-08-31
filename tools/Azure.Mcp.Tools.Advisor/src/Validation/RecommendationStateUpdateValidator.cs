// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class RecommendationStateUpdateValidator
{
    public static void AddValidationErrors(
        RecommendationStatus recommendationStatus,
        DateTimeOffset? postponedUntilDateTime,
        RecommendationDismissReason? recommendationDismissReason,
        ICollection<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (recommendationStatus == RecommendationStatus.Postponed)
        {
            if (postponedUntilDateTime is null)
            {
                errors.Add("--postponed-until-date-time is required when --recommendation-status is Postponed.");
            }
            else if (postponedUntilDateTime <= DateTimeOffset.UtcNow)
            {
                errors.Add("--postponed-until-date-time must be in the future.");
            }
        }

        if (recommendationStatus != RecommendationStatus.Dismissed &&
            recommendationDismissReason is not null)
        {
            errors.Add(
                "--recommendation-dismiss-reason can only be used when --recommendation-status is Dismissed.");
        }
    }

    public static void Validate(
        RecommendationStatus recommendationStatus,
        DateTimeOffset? postponedUntilDateTime,
        RecommendationDismissReason? recommendationDismissReason)
    {
        var errors = new List<string>();
        AddValidationErrors(
            recommendationStatus,
            postponedUntilDateTime,
            recommendationDismissReason,
            errors);

        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors));
        }
    }

    public static RecommendationDismissReason? ResolveDismissReason(
        RecommendationStatus recommendationStatus,
        RecommendationDismissReason? recommendationDismissReason) =>
        recommendationStatus == RecommendationStatus.Dismissed
            ? recommendationDismissReason ?? RecommendationDismissReason.Other
            : recommendationDismissReason;
}
