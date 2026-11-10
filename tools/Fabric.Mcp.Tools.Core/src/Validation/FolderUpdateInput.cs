// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class FolderUpdateInput
{
    internal static void Validate(string? workspaceId, string? folderId, string? displayName, ValidationResult validation)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            validation.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
        if (!Guid.TryParse(folderId, out var folder) || folder == Guid.Empty)
        {
            validation.Errors.Add("--folder-id must be a nonempty UUID.");
        }
        FolderDisplayName.Validate(displayName, validation);
    }
}
