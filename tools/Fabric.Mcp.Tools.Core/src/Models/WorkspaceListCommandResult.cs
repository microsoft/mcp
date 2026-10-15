// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record WorkspaceListCommandResult(
    List<FabricWorkspaceListEntry> Workspaces,
    string? ContinuationToken,
    string? ContinuationUri);
