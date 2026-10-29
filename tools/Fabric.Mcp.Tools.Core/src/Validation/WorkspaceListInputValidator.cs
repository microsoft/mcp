// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class WorkspaceListInputValidator
{
    internal const string RolesError = "--roles must be a comma-separated list of Admin, Member, Contributor, or Viewer without empty entries.";
    internal const string ContinuationTokenError = "--continuation-token must not be empty or whitespace. Omit it to request the first page.";

    private static readonly string[] s_roles = ["Admin", "Member", "Contributor", "Viewer"];

    internal static bool TryNormalizeRoles(string? roles, out string? normalizedRoles)
    {
        normalizedRoles = null;
        if (roles is null)
        {
            return true;
        }

        List<string> normalized = [];
        foreach (var role in roles.Split(','))
        {
            var candidate = role.Trim();
            var canonical = s_roles.FirstOrDefault(value => value.Equals(candidate, StringComparison.OrdinalIgnoreCase));
            if (canonical is null)
            {
                return false;
            }

            if (!normalized.Contains(canonical))
            {
                normalized.Add(canonical);
            }
        }

        normalizedRoles = string.Join(',', normalized);
        return true;
    }

    internal static bool IsValidContinuationToken(string? continuationToken) =>
        continuationToken is null || !string.IsNullOrWhiteSpace(continuationToken);
}
