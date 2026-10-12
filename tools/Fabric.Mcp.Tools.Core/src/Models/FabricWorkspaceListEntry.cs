// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed class FabricWorkspaceListEntry()
{
    public required Guid Id { get; init; }

    public required string DisplayName { get; init; }

    public string? Type { get; init; }

    public string? Description { get; init; }

    public Guid? CapacityId { get; init; }

    public string? CapacityRegion { get; init; }

    public Guid? DomainId { get; init; }

    public List<FabricWorkspaceListTag>? Tags { get; init; }

    public string? ApiEndpoint { get; init; }
}
