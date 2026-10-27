// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;

internal static class GoalAssignmentValidation
{
    public static void ValidateServiceLevelResourceIds(IReadOnlyList<string>? ids, string serviceGroup, ValidationResult validationResult)
    {
        ValidateResourceIds(ids, validationResult);
        foreach (string id in ids ?? [])
        {
            if (!IsResourceId(id))
            {
                continue;
            }

            var resource = new ResourceIdentifier(id);
            if (resource.ResourceType != new ResourceType("Microsoft.Monitor/slis") ||
                resource.Parent is not { } parent ||
                parent.ResourceType != new ResourceType("Microsoft.Management/serviceGroups") ||
                !string.Equals(parent.Name, serviceGroup, StringComparison.OrdinalIgnoreCase))
            {
                validationResult.Errors.Add("Each service-level resource must be a Microsoft.Monitor/slis resource in the requested service group.");
                break;
            }
        }
    }

    public static void ValidateResourceIds(IReadOnlyList<string>? ids, ValidationResult validationResult)
    {
        if (ids is null)
        {
            return;
        }

        var uniqueIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string id in ids)
        {
            if (!IsResourceId(id) || !uniqueIds.Add(id))
            {
                validationResult.Errors.Add("Resource IDs must be unique, complete Azure resource ARM IDs.");
                return;
            }
        }
    }

    private static bool IsResourceId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(c => char.IsWhiteSpace(c) || c is '?' or '#' or '%'))
        {
            return false;
        }

        try
        {
            var id = new ResourceIdentifier(value);
            return value.StartsWith("/", StringComparison.Ordinal) &&
                !string.IsNullOrEmpty(id.Name) &&
                id.ResourceType.Namespace != "Microsoft.Resources" &&
                id.ResourceType.ToString().Contains('/');
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            return false;
        }
    }

    public static UpdateGoalResourceContent CreateResourcesContent(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || Encoding.UTF8.GetByteCount(json) > 1_048_576)
        {
            throw new ArgumentException("The resource payload must be a non-empty JSON array of at most 1 MB.");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            JsonElement resources = document.RootElement;
            if (resources.ValueKind != JsonValueKind.Array || resources.GetArrayLength() == 0)
            {
                throw new ArgumentException("The resource payload must contain at least one resource.");
            }

            var members = new List<GoalMembersData>();
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonElement resource in resources.EnumerateArray())
            {
                ValidateObject(resource, "properties");
                JsonElement properties = RequiredObject(resource, "properties");
                ValidateObject(properties, "resourceArmId", "zonalResiliency");
                string resourceId = RequiredString(properties, "resourceArmId");
                if (!IsResourceId(resourceId) || !ids.Add(resourceId))
                {
                    throw new ArgumentException("Each resourceArmId must be a unique, complete Azure resource ARM ID.");
                }

                JsonElement zonal = RequiredObject(properties, "zonalResiliency");
                ValidateObject(zonal, "goalParticipation", "attestationStatus", "userConfirmation");
                var resiliency = new GoalResourceResiliencyProperties();
                bool hasChange = false;
                if (zonal.TryGetProperty("goalParticipation", out _))
                {
                    string participation = RequiredString(zonal, "goalParticipation");
                    RequireValue(participation, "Included", "Excluded");
                    resiliency.GoalParticipation = new GoalResourceExclusionState(participation);
                    hasChange = true;
                }

                if (zonal.TryGetProperty("attestationStatus", out _))
                {
                    string attestation = RequiredString(zonal, "attestationStatus");
                    RequireValue(attestation, "NotAttested", "ManuallyAttested");
                    resiliency.AttestationStatus = new GoalResourceAttestationState(attestation);
                    hasChange = true;
                }

                if (zonal.TryGetProperty("userConfirmation", out JsonElement confirmations))
                {
                    if (confirmations.ValueKind != JsonValueKind.Array || confirmations.GetArrayLength() == 0)
                    {
                        throw new ArgumentException("userConfirmation must be a non-empty array.");
                    }

                    var solutions = new HashSet<string>(StringComparer.Ordinal);
                    foreach (JsonElement confirmation in confirmations.EnumerateArray())
                    {
                        ValidateObject(confirmation, "solutionDisplayName", "confirmationStatus", "reasonForRequestingConfirmation");
                        string solution = RequiredString(confirmation, "solutionDisplayName");
                        RequireValue(solution, "ZonePinnedVmWithZrsDisk", "VmInMultiZoneVmss");
                        if (!solutions.Add(solution))
                        {
                            throw new ArgumentException("Each solution may occur only once in userConfirmation.");
                        }

                        string status = RequiredString(confirmation, "confirmationStatus");
                        RequireValue(status, "ApprovedByUser", "ApprovalPending", "ApprovalNotNeeded", "RejectedByUser");
                        var model = new GoalResourceUserConfirmation(
                            new ResilienceManagementSolutionDisplayName(solution),
                            new GoalResourceConfirmationStatus(status));
                        if (confirmation.TryGetProperty("reasonForRequestingConfirmation", out _))
                        {
                            string reason = RequiredString(confirmation, "reasonForRequestingConfirmation");
                            RequireValue(reason, "ZonePinnedZrsDataDisksConditional", "VmInMultiZoneScaleSetStatelessOnly");
                            model.ReasonForRequestingConfirmation = new GoalResourceConfirmationReason(reason);
                        }

                        resiliency.UserConfirmation.Add(model);
                    }

                    hasChange = true;
                }

                if (!hasChange)
                {
                    throw new ArgumentException("Each zonalResiliency object must specify at least one change.");
                }

                members.Add(new GoalMembersData
                {
                    Properties = new GoalResourceProperties(new ResourceIdentifier(resourceId))
                    {
                        ZonalResiliency = resiliency
                    }
                });
            }

            return new UpdateGoalResourceContent(members);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("The resource payload must be valid JSON.", ex);
        }
    }

    private static void ValidateObject(JsonElement element, params string[] allowedProperties)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("Resource payload entries and nested properties must be objects.");
        }

        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!allowedProperties.Contains(property.Name, StringComparer.Ordinal) || !names.Add(property.Name))
            {
                throw new ArgumentException("The resource payload contains an unsupported or duplicate property. Use the GA zonalResiliency schema.");
            }
        }
    }

    private static JsonElement RequiredObject(JsonElement parent, string name) =>
        parent.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Object
            ? value
            : throw new ArgumentException($"The resource payload requires a {name} object.");

    private static string RequiredString(JsonElement parent, string name) =>
        parent.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String &&
        !string.IsNullOrWhiteSpace(value.GetString())
            ? value.GetString()!
            : throw new ArgumentException($"The resource payload requires a non-empty {name} string.");

    private static void RequireValue(string value, params string[] allowed)
    {
        if (!allowed.Contains(value, StringComparer.Ordinal))
        {
            throw new ArgumentException($"The resource payload value must be one of: {string.Join(", ", allowed)}.");
        }
    }
}
