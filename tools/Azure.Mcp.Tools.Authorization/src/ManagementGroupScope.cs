// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Authorization;

/// <summary>
/// Helpers for recognizing management group scopes.
/// </summary>
/// <remarks>
/// Management group scopes are not part of any subscription, so they need different handling from
/// subscription, resource group, and resource scopes.
/// </remarks>
internal static class ManagementGroupScope
{
    private const string Prefix = "/providers/Microsoft.Management/managementGroups/";

    /// <summary>
    /// Extracts the management group ID when the scope refers to a management group itself.
    /// </summary>
    /// <param name="scope">The requested scope.</param>
    /// <param name="managementGroup">The management group ID, when the scope is a management group scope.</param>
    /// <returns><c>true</c> when the scope is a management group scope; otherwise <c>false</c>.</returns>
    public static bool TryParse(string? scope, out string managementGroup)
    {
        managementGroup = string.Empty;
        if (string.IsNullOrEmpty(scope) || !scope.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var remainder = scope[Prefix.Length..].Trim('/');
        if (remainder.Length == 0 || remainder.Contains('/'))
        {
            // A nested provider path below a management group is a resource scope, not a management group scope.
            return false;
        }

        managementGroup = remainder;
        return true;
    }
}
