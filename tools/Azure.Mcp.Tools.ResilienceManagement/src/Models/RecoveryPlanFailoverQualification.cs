// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanFailoverQualification(
    string RecoveryResourceId,
    string RecoveryResourceUniqueId,
    string? AzureResourceId,
    string? AzureResourceLocation,
    string QualificationState,
    IReadOnlyList<string> NotQualifiedReasons,
    IReadOnlyList<string> ResourcePhysicalZones,
    string? InclusionState,
    string? ProtectionStatus,
    bool? IsAttentionRequired,
    IReadOnlyList<string> AttentionReasons);
