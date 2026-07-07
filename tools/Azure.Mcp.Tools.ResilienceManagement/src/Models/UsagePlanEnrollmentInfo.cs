using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record UsagePlanEnrollmentInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("properties")] UsagePlanEnrollmentInfoProperties? Properties = null,
    [property: JsonPropertyName("systemData")] UsagePlanEnrollmentInfoSystemData? SystemData = null);

public sealed record UsagePlanEnrollmentInfoProperties(
    [property: JsonPropertyName("serviceGroupId")] string ServiceGroupId,
    [property: JsonPropertyName("provisioningState")] string ProvisioningState,
    [property: JsonPropertyName("errorDetails")] UsagePlanEnrollmentInfoErrorDetails? ErrorDetails = null);

public sealed record UsagePlanEnrollmentInfoErrorDetails(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("message")] string Message);

public sealed record UsagePlanEnrollmentInfoSystemData(
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("createdBy")] string CreatedBy,
    [property: JsonPropertyName("createdByType")] string CreatedByType,
    [property: JsonPropertyName("lastModifiedAt")] string LastModifiedAt,
    [property: JsonPropertyName("lastModifiedBy")] string LastModifiedBy,
    [property: JsonPropertyName("lastModifiedByType")] string LastModifiedByType);
