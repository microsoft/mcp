// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>
/// The metadata returned by Update Item, excluding definitions and workload-specific properties.
/// </summary>
/// <param name="Id">The item UUID.</param>
/// <param name="DisplayName">The item display name.</param>
/// <param name="Type">The item type, including types introduced by the service in the future.</param>
/// <param name="WorkspaceId">The containing workspace UUID.</param>
/// <param name="Description">The optional item description.</param>
public sealed record ItemUpdateMetadata(
    [property: JsonRequired] string Id,
    [property: JsonRequired] string DisplayName,
    [property: JsonRequired] string Type,
    [property: JsonRequired] string WorkspaceId,
    string? Description = null);
