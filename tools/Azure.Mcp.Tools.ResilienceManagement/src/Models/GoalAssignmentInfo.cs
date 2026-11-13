// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using System.Text.Json;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalAssignmentInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("properties")] GoalAssignmentInfoProperties? Properties = null,
    [property: JsonPropertyName("systemData")] GoalAssignmentInfoSystemData? SystemData = null);

public sealed record GoalAssignmentInfoProperties(
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null,
    [property: JsonPropertyName("requireZonalResiliency")] bool? RequireZonalResiliency = null,
    [property: JsonPropertyName("goalAssignmentType")] string? GoalAssignmentType = null,
    [property: JsonPropertyName("goalTemplateId")] string? GoalTemplateId = null,
    [property: JsonPropertyName("requireRegionalResiliency")] bool? RequireRegionalResiliency = null,
    [property: JsonPropertyName("regionalObjectives")] GoalAssignmentRegionalObjectives? RegionalObjectives = null,
    [property: JsonPropertyName("serviceLevelResources")] IReadOnlyList<GoalServiceLevelTargetInfo>? ServiceLevelResources = null,
    [property: JsonPropertyName("errorDetails")] JsonElement? ErrorDetails = null);

public sealed record GoalAssignmentRegionalObjectives(
    [property: JsonPropertyName("targetRecoveryPointObjective")] string TargetRecoveryPointObjective,
    [property: JsonPropertyName("targetRecoveryTimeObjective")] string TargetRecoveryTimeObjective);

public sealed record GoalAssignmentInfoSystemData(
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("createdBy")] string CreatedBy,
    [property: JsonPropertyName("createdByType")] string CreatedByType,
    [property: JsonPropertyName("lastModifiedAt")] string LastModifiedAt,
    [property: JsonPropertyName("lastModifiedBy")] string LastModifiedBy,
    [property: JsonPropertyName("lastModifiedByType")] string LastModifiedByType);
