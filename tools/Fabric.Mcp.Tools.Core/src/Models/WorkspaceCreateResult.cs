// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>The synchronous result of creating a Fabric workspace.</summary>
/// <param name="Workspace">The workspace metadata returned by Fabric.</param>
/// <param name="Location">The optional, unfollowed Location header returned by Fabric.</param>
public sealed record WorkspaceCreateResult(CreatedWorkspaceMetadata Workspace, string? Location);
