// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class WorkspaceCreateInputValidator
{
    public static IEnumerable<string> GetErrors(
        string? displayName,
        string? description,
        string? capacityId,
        string? domainId)
    {
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 256)
        {
            yield return "--display-name must not be empty or whitespace and must be at most 256 characters.";
        }
        else if (string.Equals(displayName.Trim(), "Admin monitoring", StringComparison.OrdinalIgnoreCase))
        {
            yield return "--display-name cannot use the reserved workspace name 'Admin monitoring'.";
        }

        if (description is { Length: > 4000 })
        {
            yield return "--description must be at most 4000 characters.";
        }

        if (capacityId is not null && (!Guid.TryParse(capacityId, out var capacity) || capacity == Guid.Empty))
        {
            yield return "--capacity-id must be a nonempty UUID when supplied.";
        }

        if (domainId is not null && (!Guid.TryParse(domainId, out var domain) || domain == Guid.Empty))
        {
            yield return "--domain-id must be a nonempty UUID when supplied.";
        }
    }
}
