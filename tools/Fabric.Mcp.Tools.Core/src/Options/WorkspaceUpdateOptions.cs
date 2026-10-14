// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

/// <summary>Options for updating a Fabric workspace's display name or description.</summary>
public sealed class WorkspaceUpdateOptions()
{
    /// <summary>Gets or sets the workspace UUID.</summary>
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; workspace names are not accepted.")]
    public required string WorkspaceId { get; set; }

    /// <summary>Gets or sets the new display name, or null to leave it unchanged.</summary>
    [Option(Description = "The new workspace display name, up to 256 characters. Must not be empty or whitespace-only and must be unique within the tenant. Admin monitoring is reserved. Omit to leave unchanged.")]
    public string? DisplayName { get; set; }

    /// <summary>Gets or sets the new description, or null to leave it unchanged.</summary>
    [Option(
        Description = "The new workspace description, up to 4000 characters. An empty string clears the description; omit to leave unchanged.",
        AllowEmptyOrWhiteSpaceString = true)]
    public string? Description { get; set; }
}
