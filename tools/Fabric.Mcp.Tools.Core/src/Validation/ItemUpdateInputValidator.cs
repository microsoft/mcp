// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class ItemUpdateInputValidator
{
    internal static void Validate(
        string? workspaceId,
        string? itemId,
        string? displayName,
        string? description,
        ValidationResult validationResult)
    {
        if (!Guid.TryParse(workspaceId, out var workspaceGuid) || workspaceGuid == Guid.Empty)
        {
            validationResult.Errors.Add("--workspace-id must be a nonempty UUID.");
        }

        if (!Guid.TryParse(itemId, out var itemGuid) || itemGuid == Guid.Empty)
        {
            validationResult.Errors.Add("--item-id must be a nonempty UUID.");
        }

        if (displayName is null && description is null)
        {
            validationResult.Errors.Add("Provide at least one of --display-name or --description.");
        }

        if (displayName is not null && string.IsNullOrWhiteSpace(displayName))
        {
            validationResult.Errors.Add("--display-name must not be empty or whitespace.");
        }

        if (description is { Length: > 256 })
        {
            validationResult.Errors.Add("--description must not exceed 256 characters.");
        }
    }
}
