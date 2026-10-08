// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>OneLake endpoint metadata; these addresses are not contacted by Get Workspace.</summary>
public sealed class FabricWorkspaceOneLakeEndpoints()
{
    /// <summary>Gets or sets the Blob API endpoint when available.</summary>
    public string? BlobEndpoint { get; set; }

    /// <summary>Gets or sets the DFS API endpoint when available.</summary>
    public string? DfsEndpoint { get; set; }
}
