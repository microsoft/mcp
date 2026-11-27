// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>The metadata for the requested workspace.</summary>
/// <param name="Workspace">The workspace returned by Fabric.</param>
public sealed record WorkspaceGetCommandResult(FabricWorkspaceMetadata Workspace);
