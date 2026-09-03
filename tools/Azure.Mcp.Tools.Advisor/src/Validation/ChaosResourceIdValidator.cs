// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Validation;

internal static class ChaosResourceIdValidator
{
    public static bool IsWorkspace(string? value) =>
        HasExactShape(value, expectedSegmentCount: 8);

    public static bool IsScenario(string? value) =>
        HasExactShape(value, expectedSegmentCount: 10);

    public static bool IsConfiguration(string? value) =>
        HasExactShape(value, expectedSegmentCount: 12);

    private static bool HasExactShape(string? value, int expectedSegmentCount)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 2048 ||
            !value.StartsWith("/", StringComparison.Ordinal) ||
            value.StartsWith("//", StringComparison.Ordinal) ||
            value.Contains('?', StringComparison.Ordinal) ||
            value.Contains('#', StringComparison.Ordinal) ||
            value.Contains('\\', StringComparison.Ordinal) ||
            value.Contains('%', StringComparison.Ordinal) ||
            value.Any(char.IsControl))
        {
            return false;
        }

        var segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != expectedSegmentCount ||
            !string.Equals(segments[0], "subscriptions", StringComparison.OrdinalIgnoreCase) ||
            !Guid.TryParse(segments[1], out var subscriptionId) ||
            subscriptionId == Guid.Empty ||
            !string.Equals(segments[2], "resourceGroups", StringComparison.OrdinalIgnoreCase) ||
            !IsSafeSegment(segments[3]) ||
            !string.Equals(segments[4], "providers", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(segments[5], "Microsoft.Chaos", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(segments[6], "workspaces", StringComparison.OrdinalIgnoreCase) ||
            !IsSafeSegment(segments[7]))
        {
            return false;
        }

        if (expectedSegmentCount >= 10 &&
            (!string.Equals(segments[8], "scenarios", StringComparison.OrdinalIgnoreCase) ||
             !IsSafeSegment(segments[9])))
        {
            return false;
        }

        return expectedSegmentCount < 12 ||
            string.Equals(segments[10], "configurations", StringComparison.OrdinalIgnoreCase) &&
            IsSafeSegment(segments[11]);
    }

    private static bool IsSafeSegment(string value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Length <= 260 &&
        value is not "." and not ".." &&
        !value.Any(character =>
            character is '/' or '\\' or '%' or '?' or '#' or '<' or '>' or '&' or ':') &&
        !value.Any(char.IsControl);
}
