// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record RecoveryPlanInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("planType")] string PlanType,
    [property: JsonPropertyName("planDescription")] string PlanDescription,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("planState")] string? PlanState,
    [property: JsonPropertyName("identity")] RecoveryPlanIdentityInfo? Identity,
    [property: JsonPropertyName("defaultGroup")] RecoveryPlanGroupInfo? DefaultGroup,
    [property: JsonPropertyName("additionalGroups")] IReadOnlyList<RecoveryPlanGroupInfo> AdditionalGroups);