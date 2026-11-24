// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Azure.Core;

namespace Azure.Mcp.Tools.ResourceHealth.Models;

public static partial class ResourceHealthTelemetryTags
{
    private const int MaxResourceTypeLength = 256;

    private static string AddPrefix(string tagName) => $"resourcehealth/{tagName}";

    public static readonly string ResourceType = AddPrefix("ResourceType");

    /// <summary>
    /// Adds the ARM resource type (e.g. Microsoft.Compute/virtualMachines) parsed from <paramref name="resourceId"/>.
    /// Only the type is recorded, never resource names, and only when it looks like a well-formed ARM type,
    /// since the resource ID is model-supplied and could contain arbitrary content.
    /// </summary>
    public static void AddResourceTypeTag(Activity? activity, string? resourceId)
    {
        if (activity is null || string.IsNullOrWhiteSpace(resourceId))
        {
            return;
        }

        if (!ResourceIdentifier.TryParse(resourceId, out var identifier) || identifier is null)
        {
            return;
        }

        string resourceType;
        try
        {
            resourceType = identifier.ResourceType.ToString();
        }
        catch (Exception)
        {
            return;
        }

        if (resourceType.Length <= MaxResourceTypeLength && ArmResourceTypeRegex().IsMatch(resourceType))
        {
            activity.SetTag(ResourceType, resourceType);
        }
    }

    [GeneratedRegex(@"^[A-Za-z0-9]+(\.[A-Za-z0-9]+)+(/[A-Za-z0-9-]+)+$", RegexOptions.CultureInvariant)]
    private static partial Regex ArmResourceTypeRegex();
}
