// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>The metadata for the requested Fabric capacity.</summary>
/// <param name="Capacity">The capacity metadata.</param>
public sealed record CapacityGetCommandResult(FabricCapacityMetadata Capacity);
