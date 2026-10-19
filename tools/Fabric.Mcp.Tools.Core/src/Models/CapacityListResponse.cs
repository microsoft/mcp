// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One page returned by the Fabric List Capacities API.</summary>
public sealed class CapacityListResponse()
{
    public required List<FabricCapacityMetadata> Value { get; init; }

    public string? ContinuationToken { get; init; }

    public string? ContinuationUri { get; init; }
}
