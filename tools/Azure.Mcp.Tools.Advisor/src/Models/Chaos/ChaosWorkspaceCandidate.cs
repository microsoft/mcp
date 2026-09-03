// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models.Chaos;

/// <summary>Hydrated Chaos workspace candidate that covers the selected VMSS.</summary>
public sealed record ChaosWorkspaceCandidate(
    string Id,
    string Name,
    string? Location,
    string? ProvisioningState,
    string? IdentityType,
    string? PrincipalId,
    IReadOnlyList<string> Scopes,
    bool Selectable,
    string? ReasonCode = null);
