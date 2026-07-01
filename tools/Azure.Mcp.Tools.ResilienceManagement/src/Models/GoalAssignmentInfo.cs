using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalAssignmentInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("properties")] GoalAssignmentInfoProperties? Properties = null,
    [property: JsonPropertyName("systemData")] GoalAssignmentInfoSystemData? SystemData = null);

public sealed record GoalAssignmentInfoProperties(
    [property: JsonPropertyName("goalAssignmentType")] string GoalAssignmentType,
    [property: JsonPropertyName("goalTemplateId")] string GoalTemplateId,
    [property: JsonPropertyName("provisioningState")] string ProvisioningState);

public sealed record GoalAssignmentInfoSystemData(
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("createdBy")] string CreatedBy,
    [property: JsonPropertyName("createdByType")] string CreatedByType,
    [property: JsonPropertyName("lastModifiedAt")] string LastModifiedAt,
    [property: JsonPropertyName("lastModifiedBy")] string LastModifiedBy,
    [property: JsonPropertyName("lastModifiedByType")] string LastModifiedByType);
