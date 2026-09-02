// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class RecommendationStateUpdateValidator
{
    public static void AddCommandValidationErrors(
        RecommendationStatus recommendationStatus,
        string? postponedUntilDateTime,
        RecommendationDismissReason? recommendationDismissReason,
        ICollection<string> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        if (recommendationStatus != RecommendationStatus.Postponed &&
            postponedUntilDateTime is not null)
        {
            errors.Add(
                "--postponed-until-date-time can only be used when --recommendation-status is Postponed.");
            AddDismissReasonValidationError(
                recommendationStatus,
                recommendationDismissReason,
                errors);
            return;
        }

        if (!TryParsePostponedUntilDateTime(
            postponedUntilDateTime,
            out var parsedPostponedUntilDateTime,
            out var error) &&
            recommendationStatus == RecommendationStatus.Postponed)
        {
            errors.Add(error!);
            AddDismissReasonValidationError(
                recommendationStatus,
                recommendationDismissReason,
                errors);
            return;
        }

        AddValidationErrors(
            recommendationStatus,
            parsedPostponedUntilDateTime,
            recommendationDismissReason,
            errors);
    }

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
        else if (postponedUntilDateTime is not null)
        {
            errors.Add(
                "--postponed-until-date-time can only be used when --recommendation-status is Postponed.");
        }

        AddDismissReasonValidationError(
            recommendationStatus,
            recommendationDismissReason,
            errors);
    }

    public static bool TryParsePostponedUntilDateTime(
        string? value,
        out DateTimeOffset? postponedUntilDateTime,
        out string? error)
    {
        postponedUntilDateTime = null;
        error = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            error = "--postponed-until-date-time is required when --recommendation-status is Postponed.";
            return false;
        }

        var normalized = value.Trim();
        if (!HasExplicitOffset(normalized))
        {
            error = "--postponed-until-date-time must end in 'Z' or an explicit timezone offset such as '+05:30'.";
            return false;
        }

        if (!DateTimeOffset.TryParse(
            normalized,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed))
        {
            error = "--postponed-until-date-time must be a valid ISO 8601 date and time.";
            return false;
        }

        postponedUntilDateTime = parsed;
        return true;
    }

    private static void AddDismissReasonValidationError(
        RecommendationStatus recommendationStatus,
        RecommendationDismissReason? recommendationDismissReason,
        ICollection<string> errors)
    {
        if (recommendationStatus != RecommendationStatus.Dismissed &&
            recommendationDismissReason is not null)
        {
            errors.Add(
                "--recommendation-dismiss-reason can only be used when --recommendation-status is Dismissed.");
        }
    }

    private static bool HasExplicitOffset(string value)
    {
        if (value.EndsWith('Z') || value.EndsWith('z'))
        {
            return true;
        }

        if (value.Length < 6)
        {
            return false;
        }

        var offset = value.AsSpan(value.Length - 6);
        return (offset[0] is '+' or '-') &&
            char.IsAsciiDigit(offset[1]) &&
            char.IsAsciiDigit(offset[2]) &&
            offset[3] == ':' &&
            char.IsAsciiDigit(offset[4]) &&
            char.IsAsciiDigit(offset[5]);
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
