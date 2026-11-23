// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Move Item metadata. Only type is required by the API; unavailable fields are not inferred.</summary>
public sealed record FabricMovedItem(
    [property: JsonRequired] string Type,
    Guid? Id = null,
    string? DisplayName = null,
    Guid? WorkspaceId = null,
    string? Description = null,
    Guid? FolderId = null,
    Guid? LogicalId = null,
    List<FabricItemTag>? Tags = null,
    FabricItemSensitivityLabel? SensitivityLabel = null);
