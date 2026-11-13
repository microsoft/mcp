// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Documented identity metadata returned by Fabric, without directory enrichment.</summary>
public sealed record FabricPrincipal(
    Guid Id,
    string Type,
    string? DisplayName = null,
    FabricUserDetails? UserDetails = null,
    FabricGroupDetails? GroupDetails = null,
    FabricServicePrincipalDetails? ServicePrincipalDetails = null,
    FabricServicePrincipalProfileDetails? ServicePrincipalProfileDetails = null);
