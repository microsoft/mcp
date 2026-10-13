// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

/// <summary>Options for creating one Microsoft Fabric workspace.</summary>
public sealed class WorkspaceCreateOptions()
{
    /// <summary>Gets or sets the new workspace's display name.</summary>
    [Option(Description = "The display name of the new Microsoft Fabric workspace. Required, not empty or whitespace, and at most 256 characters. Only unused names are allowed; 'Admin monitoring' is reserved.")]
    public required string DisplayName { get; set; }

    /// <summary>Gets or sets the optional workspace description.</summary>
    [Option(Description = "The workspace description, at most 4000 characters. Omit to use the service default.")]
    public string? Description { get; set; }

    /// <summary>Gets or sets the existing capacity's ID.</summary>
    [Option(Description = "The nonempty UUID of an existing capacity to assign during workspace creation. Requires capacity contributor or admin permission. Does not create a capacity or perform a separate assignment operation.")]
    public string? CapacityId { get; set; }

    /// <summary>Gets or sets the existing domain's ID.</summary>
    [Option(Description = "The nonempty UUID of an existing domain to assign during workspace creation. Requires permission to assign the workspace to that domain. Does not create or look up a domain.")]
    public string? DomainId { get; set; }
}
