// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed class WorkspaceListResponse()
{
    public required List<FabricWorkspaceListEntry> Value { get; init; }

    public string? ContinuationToken { get; init; }

    public string? ContinuationUri { get; init; }
}
