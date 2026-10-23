// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanGroupInput(
    string? GroupUniqueId,
    int OrderId,
    string Description,
    IReadOnlyList<RecoveryPlanGroupActionInput>? PreActions = null,
    IReadOnlyList<RecoveryPlanGroupActionInput>? PostActions = null);
