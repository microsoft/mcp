// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Services;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class ItemDefinitionGetInput
{
    internal static (Guid? WorkspaceId, Guid? ItemId, Guid? OperationId) Validate(
        string? workspaceId, string? itemId, string? format, string? operationId, FabricOperationWaitOptions waitOptions)
    {
        waitOptions.Validate();
        if (operationId is not null)
        {
            if (!Guid.TryParse(operationId, out var operation) || operation == Guid.Empty)
            {
                throw new ArgumentException("--operation-id must be a nonempty UUID from an earlier definition retrieval.");
            }
            if (workspaceId is not null || itemId is not null || format is not null || waitOptions.Sync)
            {
                throw new ArgumentException("--operation-id reads a completed definition result once and cannot be combined with --workspace-id, --item-id, --format, or --sync true.");
            }
            return (null, null, operation);
        }
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Provide a nonempty workspace UUID using --workspace-id; names are not resolved.");
        }
        if (!Guid.TryParse(itemId, out var item) || item == Guid.Empty)
        {
            throw new ArgumentException("Provide a nonempty item UUID using --item-id; names are not resolved.");
        }
        if (format is not null && (string.IsNullOrWhiteSpace(format) || format.Length > 128 || format.Any(char.IsControl)))
        {
            throw new ArgumentException("--format must not be empty or whitespace, must be at most 128 characters, and contain no control characters.");
        }
        return (workspace, item, null);
    }
}
