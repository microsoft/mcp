// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace Azure.Mcp.Tools.Optimization.Services;

/// <summary>
/// Minimal Azure ARM resource-id helpers: validation and leaf resource-name extraction.
/// </summary>
public static partial class ArmResourceId
{
    [GeneratedRegex(
        @"^/subscriptions/(?<subscriptionId>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})/resourceGroups/(?<resourceGroupName>[^/]+)/providers/(?<providerNamespace>[^/]+)/(?<resourceType>[^/]+)/(?<resourceName>[^/]+)(/(?<subResourceType>[^/]+)/(?<subResourceName>[^/]+))*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex ResourceGroupScoped();

    [GeneratedRegex(
        @"^/subscriptions/(?<subscriptionId>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})/providers/(?<providerNamespace>[^/]+)/(?<resourceType>[^/]+)/(?<resourceName>[^/]+)(/(?<subResourceType>[^/]+)/(?<subResourceName>[^/]+))*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SubscriptionScoped();

    /// <summary>Returns true when the value is a valid resource-group- or subscription-scoped ARM id.</summary>
    public static bool IsValid(string? resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            return false;
        }

        return ResourceGroupScoped().IsMatch(resourceId) || SubscriptionScoped().IsMatch(resourceId);
    }

    /// <summary>Extracts the leaf resource name, or null when the id is not a valid ARM id.</summary>
    public static string? ExtractResourceName(string? resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            return null;
        }

        var match = ResourceGroupScoped().Match(resourceId);
        if (match.Success && match.Groups["resourceName"].Success)
        {
            return match.Groups["resourceName"].Value;
        }

        match = SubscriptionScoped().Match(resourceId);
        return match.Success && match.Groups["resourceName"].Success
            ? match.Groups["resourceName"].Value
            : null;
    }

    /// <summary>
    /// Returns the impacted resource id from an Advisor recommendation id by removing any trailing
    /// <c>/providers/Microsoft.Advisor</c> segment (with or without a
    /// <c>/recommendations/&lt;id&gt;</c> suffix). Non-Advisor ids are returned unchanged, so callers
    /// can pass either the recommendation id or the resource id.
    /// </summary>
    public static string StripAdvisorRecommendationSuffix(string resourceId)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            return resourceId;
        }

        const string marker = "/providers/microsoft.advisor";
        var index = resourceId.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return resourceId;
        }

        // Only strip when the marker is a complete path segment (end of string or followed by '/'),
        // so a namespace like 'Microsoft.AdvisorX' is not matched.
        var afterMarker = index + marker.Length;
        var isSegmentBoundary = afterMarker == resourceId.Length || resourceId[afterMarker] == '/';
        return isSegmentBoundary ? resourceId[..index] : resourceId;
    }
}
