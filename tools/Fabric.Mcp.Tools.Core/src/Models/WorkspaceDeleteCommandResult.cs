// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Acknowledges Fabric's successful response to the targeted workspace deletion.</summary>
/// <param name="WorkspaceId">The workspace UUID supplied to the deletion request.</param>
/// <param name="Deleted">Whether Fabric reported successful deletion.</param>
public sealed record WorkspaceDeleteCommandResult(Guid WorkspaceId, bool Deleted);
