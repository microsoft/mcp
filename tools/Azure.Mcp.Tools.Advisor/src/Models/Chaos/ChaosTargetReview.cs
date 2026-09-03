// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models.Chaos;

/// <summary>Current read-only eligibility status for one selected VMSS.</summary>
public sealed record ChaosTargetReview(
    string Status,
    bool Eligible,
    string? ReasonCode,
    string Message,
    string? RecommendationTypeId,
    string? ResourceId,
    string? Location,
    IReadOnlyList<string> Zones,
    long? Capacity,
    string? ProvisioningState,
    string? RequiredPermission = null);
