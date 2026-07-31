// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record UsagePlanInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("resourceType")] string ResourceType,
    [property: JsonPropertyName("location")] string Location,
    [property: JsonPropertyName("tags")] IReadOnlyDictionary<string, string>? Tags = null,
    [property: JsonPropertyName("properties")] UsagePlanInfoProperties? Properties = null,
    [property: JsonPropertyName("systemData")] UsagePlanInfoSystemData? SystemData = null);

public sealed record UsagePlanInfoProperties(
    [property: JsonPropertyName("planType")] string PlanType,
    [property: JsonPropertyName("provisioningState")] string ProvisioningState);

public sealed record UsagePlanInfoSystemData(
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("createdBy")] string CreatedBy,
    [property: JsonPropertyName("lastModifiedAt")] string LastModifiedAt,
    [property: JsonPropertyName("lastModifiedBy")] string LastModifiedBy);
