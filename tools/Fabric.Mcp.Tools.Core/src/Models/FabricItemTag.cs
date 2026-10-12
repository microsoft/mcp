// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A tag applied to a Fabric item.</summary>
/// <param name="Id">The tag UUID.</param>
/// <param name="DisplayName">The tag display name.</param>
public sealed record FabricItemTag(Guid Id, string DisplayName);
