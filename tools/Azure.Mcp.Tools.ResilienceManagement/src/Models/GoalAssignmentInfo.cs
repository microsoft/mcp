// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalAssignmentInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("properties")] GoalAssignmentInfoProperties? Properties = null,
    [property: JsonPropertyName("systemData")] GoalAssignmentInfoSystemData? SystemData = null);

public sealed record GoalAssignmentInfoProperties(
    [property: JsonPropertyName("provisioningState")] string ProvisioningState,
    [property: JsonPropertyName("requireZonalResiliency")] bool? RequireZonalResiliency = null,
    [property: JsonPropertyName("serviceLevelResources")] IReadOnlyList<GoalServiceLevelTargetInfo>? ServiceLevelResources = null,
    [property: JsonPropertyName("errorDetails")] JsonElement? ErrorDetails = null);

public sealed record GoalAssignmentInfoSystemData(
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("createdBy")] string CreatedBy,
    [property: JsonPropertyName("createdByType")] string CreatedByType,
    [property: JsonPropertyName("lastModifiedAt")] string LastModifiedAt,
    [property: JsonPropertyName("lastModifiedBy")] string LastModifiedBy,
    [property: JsonPropertyName("lastModifiedByType")] string LastModifiedByType);
