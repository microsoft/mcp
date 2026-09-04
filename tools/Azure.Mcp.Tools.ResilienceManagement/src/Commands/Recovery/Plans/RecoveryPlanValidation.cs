// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.ResilienceManagement;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

internal static class RecoveryPlanValidation
{
    private static readonly ResourceType ServiceGroupResourceType = new("Microsoft.Management/serviceGroups");

    public static void ValidateServiceGroup(string serviceGroup, ValidationResult validationResult)
    {
        if (serviceGroup.Length is < 1 or > 90 || !serviceGroup.All(IsValidServiceGroupNameCharacter))
        {
            validationResult.Errors.Add("The service group name must be 1 to 90 characters and contain only ASCII letters, numbers, hyphens, underscores, periods, or parentheses.");
        }
    }

    public static void ValidateName(string recoveryPlan, ValidationResult validationResult)
    {
        if (recoveryPlan.Length is < 5 or > 24 || !recoveryPlan.All(IsValidNameCharacter))
        {
            validationResult.Errors.Add("The recoveryplan name must be 5 to 24 characters and contain only ASCII letters, numbers, or hyphens.");
        }
    }

    public static void ValidateSelectedResourceIds(
        IReadOnlyList<string>? selectedResourceIds,
        string serviceGroup,
        string recoveryPlan,
        ValidationResult validationResult)
    {
        foreach (string resourceId in selectedResourceIds ?? [])
        {
            if (string.IsNullOrWhiteSpace(resourceId) || !IsRecoveryResourceIdForPlan(resourceId, serviceGroup, recoveryPlan))
            {
                validationResult.Errors.Add("Each --selected-resource-ids value must be a full recovery-resource ID under the requested service group and recoveryplan.");
                break;
            }
        }
    }

    private static bool IsRecoveryResourceIdForPlan(string resourceId, string serviceGroup, string recoveryPlan)
    {
        try
        {
            var parsed = new ResourceIdentifier(resourceId);
            ResourceIdentifier? recoveryPlanId = parsed.Parent;
            ResourceIdentifier? serviceGroupId = recoveryPlanId?.Parent;
            return parsed.ResourceType == RecoveryMembersResource.ResourceType &&
                recoveryPlanId?.ResourceType == RecoveryPlanResource.ResourceType &&
                string.Equals(recoveryPlanId.Name, recoveryPlan, StringComparison.OrdinalIgnoreCase) &&
                serviceGroupId?.ResourceType == ServiceGroupResourceType &&
                string.Equals(serviceGroupId.Name, serviceGroup, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            return false;
        }
    }

    private static bool IsValidNameCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-';

    private static bool IsValidServiceGroupNameCharacter(char character) =>
        IsValidNameCharacter(character) || character is '_' or '.' or '(' or ')';
}
