// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Only the workspace properties explicitly supplied for a PATCH.</summary>
public sealed class UpdateWorkspaceRequest()
{
    /// <summary>Gets or sets the display name, or null to omit it from the request.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Gets or sets the description, including an empty string to clear it, or null to omit it.</summary>
    public string? Description { get; set; }
}
