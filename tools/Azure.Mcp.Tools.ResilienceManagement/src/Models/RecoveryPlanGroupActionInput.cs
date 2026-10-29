// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanGroupActionInput(
    RecoveryPlanGroupActionKind Type,
    string Name,
    string? Description,
    int TimeoutInMinutes,
    string? ActionResourceId,
    IReadOnlyDictionary<string, string>? Parameters,
    string? AssociatedIdentityType = null,
    string? AssociatedUserAssignedIdentity = null);
