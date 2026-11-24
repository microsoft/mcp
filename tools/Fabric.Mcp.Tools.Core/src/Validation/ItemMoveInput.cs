// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class ItemMoveInput
{
    internal static void Validate(string? workspaceId, string? itemId, string? targetFolderId, ValidationResult validation)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            validation.Errors.Add("--workspace-id must be a nonempty UUID.");
        }
        if (!Guid.TryParse(itemId, out var item) || item == Guid.Empty)
        {
            validation.Errors.Add("--item-id must be a nonempty UUID.");
        }
        if (targetFolderId is not null && (!Guid.TryParse(targetFolderId, out var target) || target == Guid.Empty))
        {
            validation.Errors.Add("--target-folder-id must be a nonempty UUID when supplied; omit it to use the workspace root.");
        }
    }
}
