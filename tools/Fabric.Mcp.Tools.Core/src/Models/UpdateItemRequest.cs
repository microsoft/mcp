// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>
/// Updates only the supplied item properties. Null properties are omitted; an empty description clears it.
/// </summary>
/// <param name="DisplayName">The new display name, or null to leave it unchanged.</param>
/// <param name="Description">The new description, or null to leave it unchanged.</param>
public sealed record UpdateItemRequest(string? DisplayName = null, string? Description = null);
