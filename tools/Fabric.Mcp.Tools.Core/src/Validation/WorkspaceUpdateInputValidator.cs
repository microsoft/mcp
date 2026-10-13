// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class WorkspaceUpdateInputValidator
{
    internal static List<string> GetErrors(string? workspaceId, string? displayName, string? description)
    {
        List<string> errors = [];

        if (!Guid.TryParse(workspaceId, out var parsedId) || parsedId == Guid.Empty)
        {
            errors.Add("--workspace-id must be a nonempty UUID.");
        }

        if (displayName is not null)
        {
            if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 256)
            {
                errors.Add("--display-name must not be empty or whitespace-only and must contain no more than 256 characters.");
            }

            if (string.Equals(displayName, "Admin monitoring", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("--display-name cannot use the reserved workspace name Admin monitoring.");
            }
        }

        if (description?.Length > 4000)
        {
            errors.Add("--description must contain no more than 4000 characters.");
        }

        if (displayName is null && description is null)
        {
            errors.Add("Provide at least one update: --display-name or --description. An empty description clears it.");
        }

        return errors;
    }
}
