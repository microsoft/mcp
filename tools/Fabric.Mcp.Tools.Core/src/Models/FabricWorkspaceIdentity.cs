// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Identifiers for a workspace identity, without credentials.</summary>
public sealed class FabricWorkspaceIdentity()
{
    /// <summary>Gets or sets the application ID when available.</summary>
    public Guid? ApplicationId { get; set; }

    /// <summary>Gets or sets the service principal ID when available.</summary>
    public Guid? ServicePrincipalId { get; set; }
}
