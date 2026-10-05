// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Capacity metadata returned by the Fabric capacity APIs.</summary>
public sealed class FabricCapacityMetadata()
{
    public required Guid Id { get; set; }

    public required string DisplayName { get; set; }

    public required string Sku { get; set; }

    public required string Region { get; set; }

    public required string State { get; set; }

    internal static bool IsValid([NotNullWhen(true)] FabricCapacityMetadata? capacity) =>
        capacity is not null &&
        capacity.Id != Guid.Empty &&
        !string.IsNullOrWhiteSpace(capacity.DisplayName) &&
        !string.IsNullOrWhiteSpace(capacity.Sku) &&
        !string.IsNullOrWhiteSpace(capacity.Region) &&
        !string.IsNullOrWhiteSpace(capacity.State);
}
