// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>An applied tag returned in created workspace metadata.</summary>
public sealed class CreatedWorkspaceTag()
{
    /// <summary>Gets the tag ID.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the tag's display name.</summary>
    public required string DisplayName { get; init; }
}
