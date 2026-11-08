// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Buffers;
using System.Text;
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
        if (!IsValidDisplayName(displayName))
        {
            validation.Errors.Add("--display-name must be valid Unicode with 1 to 255 characters, no leading or trailing spaces, C0/C1 control codes, or ~\"#.&*:<>?/{|} characters. Reserved names $recycle.bin, recycled, and recycler are not allowed.");
        }
    }

    private static bool IsValidDisplayName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name) || char.IsWhiteSpace(name[0]) || char.IsWhiteSpace(name[^1]) ||
            name.Equals("$recycle.bin", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("recycled", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("recycler", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var remaining = name.AsSpan();
        var characters = 0;
        while (!remaining.IsEmpty)
        {
            if (Rune.DecodeFromUtf16(remaining, out var character, out var consumed) != OperationStatus.Done ||
                ++characters > 255 || Rune.IsControl(character) ||
                (character.IsAscii && "~\"#.&*:<>?/{|}".Contains((char)character.Value)))
            {
                return false;
            }
            remaining = remaining[consumed..];
        }
        return true;
    }
}
