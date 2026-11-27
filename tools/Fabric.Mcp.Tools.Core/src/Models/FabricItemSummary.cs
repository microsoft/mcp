// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Inventory metadata, excluding item data, definitions, and default identities.</summary>
/// <param name="Id">The item UUID.</param>
/// <param name="DisplayName">The item display name.</param>
/// <param name="Type">The item type, including types introduced by future service versions.</param>
/// <param name="WorkspaceId">The containing workspace UUID.</param>
/// <param name="Description">The optional item description.</param>
/// <param name="FolderId">The optional containing folder UUID.</param>
/// <param name="LogicalId">The optional logical ID shared by corresponding item instances.</param>
/// <param name="Tags">The optional applied tags.</param>
/// <param name="SensitivityLabel">The optional sensitivity-label metadata.</param>
public sealed record FabricItemSummary(
    Guid Id,
    string DisplayName,
    string Type,
    Guid WorkspaceId,
    string? Description = null,
    Guid? FolderId = null,
    Guid? LogicalId = null,
    List<FabricItemTag>? Tags = null,
    FabricItemSensitivityLabel? SensitivityLabel = null);
