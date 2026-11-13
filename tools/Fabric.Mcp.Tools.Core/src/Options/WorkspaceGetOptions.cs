// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

/// <summary>Options for retrieving metadata for one existing Fabric workspace.</summary>
public sealed class WorkspaceGetOptions()
{
    /// <summary>Gets or sets the workspace ID as a nonempty UUID.</summary>
    [Option(Description = "The ID of the Microsoft Fabric workspace to retrieve. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    /// <summary>Gets or sets the endpoint preference, or null to preserve the service default.</summary>
    [Option(Description = "Whether to request workspace-specific API and OneLake endpoints. Omit to preserve the service default. Returned endpoints are metadata only and are not contacted by this tool.")]
    public bool? PreferWorkspaceSpecificEndpoints { get; set; }
}
