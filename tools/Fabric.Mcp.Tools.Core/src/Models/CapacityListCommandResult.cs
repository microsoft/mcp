// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One page of capacity metadata and the unchanged continuation information.</summary>
public sealed record CapacityListCommandResult(
    List<FabricCapacityMetadata> Capacities,
    string? ContinuationToken,
    string? ContinuationUri);
