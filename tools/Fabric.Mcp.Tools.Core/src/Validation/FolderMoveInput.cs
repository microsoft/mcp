// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class FolderMoveInput
{
    internal static void Validate(string? workspaceId, string? folderId, string? targetFolderId, ValidationResult validation)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            validation.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
        if (!Guid.TryParse(folderId, out var folder) || folder == Guid.Empty)
        {
            validation.Errors.Add("--folder-id must be a nonempty UUID.");
        }
        if (targetFolderId is not null)
        {
            if (!Guid.TryParse(targetFolderId, out var target) || target == Guid.Empty)
            {
                validation.Errors.Add("--target-folder-id must be a nonempty UUID when supplied; omit it to use the workspace root.");
            }
            else if (target == folder)
            {
                validation.Errors.Add("--target-folder-id cannot identify the folder being moved.");
            }
        }
    }
}
