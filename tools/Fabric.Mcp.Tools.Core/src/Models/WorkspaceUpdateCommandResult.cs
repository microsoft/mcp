// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>The workspace metadata returned by the update command.</summary>
/// <param name="Workspace">The workspace returned by the PATCH operation.</param>
public sealed record WorkspaceUpdateCommandResult(WorkspaceUpdateResponse Workspace);
