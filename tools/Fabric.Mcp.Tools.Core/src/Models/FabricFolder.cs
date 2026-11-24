// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Logical workspace folder metadata, not a local or OneLake filesystem path.</summary>
public sealed record FabricFolder(
    Guid Id,
    string DisplayName,
    Guid WorkspaceId,
    Guid? ParentFolderId = null);
