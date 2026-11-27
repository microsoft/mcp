// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>The updated item's metadata.</summary>
/// <param name="Item">The item returned by the update operation.</param>
public sealed record ItemUpdateCommandResult(ItemUpdateMetadata Item);
