// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>The workspace metadata returned by an update, without unrelated resource properties.</summary>
public sealed class WorkspaceUpdateResponse()
{
    /// <summary>Gets or sets the workspace ID.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the workspace display name.</summary>
    public required string DisplayName { get; set; }

    /// <summary>Gets or sets the workspace type when returned, including future service values.</summary>
    public string? Type { get; set; }

    /// <summary>Gets or sets the description when returned by Fabric.</summary>
    public string? Description { get; set; }
}
