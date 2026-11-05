// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services;

internal static class UnusedAgePolicy
{
    internal const int DefaultMinimumUnusedDays = 30;
    internal const int MaximumMinimumUnusedDays = 365;

    internal static PreValidationCheckResult Evaluate(
        IReadOnlyCollection<UnusedAgeObservation> observations,
        DateTimeOffset now,
        int minimumUnusedDays)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(minimumUnusedDays, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minimumUnusedDays, MaximumMinimumUnusedDays);

        var latest = observations.Select(static observation => observation.LastActivityAt).Max();
        if (latest > now)
        {
            latest = now;
        }

        var daysUnused = latest is null ? (int?)null : (int)Math.Floor((now - latest.Value).TotalDays);
        string outcome;
        string evidence;
        if (daysUnused < minimumUnusedDays)
        {
            outcome = "warning";
            evidence = $"Last used or changed {daysUnused} day(s) ago, fewer than the required {minimumUnusedDays} days. " +
                "Recent inactivity may indicate an in-progress migration or another intentional transition; investigate before deletion.";
        }
        else if (observations.Count == 0 || observations.Any(static observation => observation.IsUnknown))
        {
            outcome = "unverified";
            evidence = $"Could not determine whether the resource has been unused for at least {minimumUnusedDays} days. " +
                "Missing or unreadable activity evidence requires additional investigation.";
        }
        else if (observations.Any(observation => observation.QuietWindowDays is int days && days < minimumUnusedDays))
        {
            outcome = "unverified";
            evidence = $"Available activity history does not cover the required {minimumUnusedDays} days; additional investigation is required.";
        }
        else
        {
            outcome = "passed";
            evidence = $"All declared activity signals were read and confirm at least {minimumUnusedDays} days without activity.";
        }

        return new("unused-age", $"Unused for at least {minimumUnusedDays} days", "scope", "warn",
            outcome, evidence, DaysUnused: daysUnused);
    }
}
