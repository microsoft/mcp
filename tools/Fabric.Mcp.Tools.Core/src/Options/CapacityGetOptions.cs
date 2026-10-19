// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

/// <summary>Options for retrieving metadata for one existing Fabric capacity.</summary>
public sealed class CapacityGetOptions()
{
    /// <summary>Gets or sets the capacity ID as a nonempty UUID.</summary>
    [Option(Description = "The ID of the Microsoft Fabric capacity to retrieve. Must be a nonempty UUID.")]
    public required string CapacityId { get; set; }
}
