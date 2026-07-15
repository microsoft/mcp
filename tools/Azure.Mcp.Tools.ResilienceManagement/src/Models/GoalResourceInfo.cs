using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResilienceManagement.Models;

public sealed record GoalResourceInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("properties")] GoalResourceInfoProperties? Properties = null,
    [property: JsonPropertyName("systemData")] GoalResourceInfoSystemData? SystemData = null);

public sealed record GoalResourceInfoProperties(
    [property: JsonPropertyName("disasterRecoveryAttestationStatus")] string DisasterRecoveryAttestationStatus,
    [property: JsonPropertyName("disasterRecoveryGoalParticipation")] string DisasterRecoveryGoalParticipation,
    [property: JsonPropertyName("exclusionReasonForDisasterRecoveryGoals")] string ExclusionReasonForDisasterRecoveryGoals,
    [property: JsonPropertyName("exclusionReasonForHighAvailabilityGoals")] string ExclusionReasonForHighAvailabilityGoals,
    [property: JsonPropertyName("highAvailabilityAttestationStatus")] string HighAvailabilityAttestationStatus,
    [property: JsonPropertyName("highAvailabilityGoalParticipation")] string HighAvailabilityGoalParticipation,
    [property: JsonPropertyName("provisioningState")] string ProvisioningState,
    [property: JsonPropertyName("resourceArmId")] string ResourceArmId,
    [property: JsonPropertyName("serviceGroupMemberships")] IReadOnlyList<GoalResourceServiceGroupMembership>? ServiceGroupMemberships = null);

public sealed record GoalResourceServiceGroupMembership(
    [property: JsonPropertyName("membershipType")] string MembershipType,
    [property: JsonPropertyName("serviceGroupId")] string ServiceGroupId);

public sealed record GoalResourceInfoSystemData(
    [property: JsonPropertyName("createdAt")] string CreatedAt,
    [property: JsonPropertyName("createdBy")] string CreatedBy,
    [property: JsonPropertyName("createdByType")] string CreatedByType,
    [property: JsonPropertyName("lastModifiedAt")] string LastModifiedAt,
    [property: JsonPropertyName("lastModifiedBy")] string LastModifiedBy,
    [property: JsonPropertyName("lastModifiedByType")] string LastModifiedByType);
