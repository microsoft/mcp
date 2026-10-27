// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalResourceInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("properties")] GoalResourceInfoProperties? Properties = null,
    [property: JsonPropertyName("systemData")] GoalResourceInfoSystemData? SystemData = null);

public sealed record GoalResourceInfoProperties(
    [property: JsonPropertyName("provisioningState")] string ProvisioningState,
    [property: JsonPropertyName("resourceArmId")] string ResourceArmId,
    [property: JsonPropertyName("zonalResiliency")] GoalResourceResiliencyInfo? ZonalResiliency = null);

public sealed record GoalResourceInfoSystemData(
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("createdBy")] string CreatedBy,
    [property: JsonPropertyName("createdByType")] string CreatedByType,
    [property: JsonPropertyName("lastModifiedAt")] string LastModifiedAt,
    [property: JsonPropertyName("lastModifiedBy")] string LastModifiedBy,
    [property: JsonPropertyName("lastModifiedByType")] string LastModifiedByType);
