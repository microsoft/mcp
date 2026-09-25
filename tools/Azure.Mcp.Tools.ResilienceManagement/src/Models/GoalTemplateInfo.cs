// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalTemplateInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("properties")] GoalTemplateInfoProperties? Properties = null,
    [property: JsonPropertyName("systemData")] GoalTemplateInfoSystemData? SystemData = null);

public sealed record GoalTemplateInfoProperties(
    [property: JsonPropertyName("goalType")] string GoalType,
    [property: JsonPropertyName("provisioningState")] string ProvisioningState,
    [property: JsonPropertyName("regionalRecoveryPointObjective")] string RegionalRecoveryPointObjective,
    [property: JsonPropertyName("regionalRecoveryTimeObjective")] string RegionalRecoveryTimeObjective,
    [property: JsonPropertyName("requireDisasterRecovery")] string RequireDisasterRecovery,
    [property: JsonPropertyName("requireHighAvailability")] string RequireHighAvailability);

public sealed record GoalTemplateInfoSystemData(
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("createdBy")] string CreatedBy,
    [property: JsonPropertyName("createdByType")] string CreatedByType,
    [property: JsonPropertyName("lastModifiedAt")] string LastModifiedAt,
    [property: JsonPropertyName("lastModifiedBy")] string LastModifiedBy,
    [property: JsonPropertyName("lastModifiedByType")] string LastModifiedByType);
