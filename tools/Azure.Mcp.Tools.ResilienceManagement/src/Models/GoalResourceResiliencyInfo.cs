// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalResourceResiliencyInfo(
    string? GoalParticipation,
    string? AttestationStatus,
    string? ExclusionReason,
    IReadOnlyList<GoalResourceConfirmationInfo> UserConfirmation);
