// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class FolderCreateInput
{
    internal static void Validate(string? workspaceId, string? displayName, string? parentFolderId, ValidationResult validation)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            validation.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
        if (parentFolderId is not null && (!Guid.TryParse(parentFolderId, out var parent) || parent == Guid.Empty))
        {
            validation.Errors.Add("--parent-folder-id must be a nonempty UUID when supplied.");
        }
        FolderDisplayName.Validate(displayName, validation);
    }
}
