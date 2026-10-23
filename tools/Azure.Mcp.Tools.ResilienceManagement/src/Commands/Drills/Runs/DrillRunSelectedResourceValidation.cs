// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.ResilienceManagement;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

internal static class DrillRunSelectedResourceValidation
{
    private static readonly ResourceType ServiceGroupResourceType =
        new("Microsoft.Management/serviceGroups");

    public static bool IsRecoveryResourceId(
        string resourceId,
        string serviceGroup)
    {
        try
        {
            var parsed = new ResourceIdentifier(resourceId);
            ResourceIdentifier? recoveryPlanId = parsed.Parent;
            ResourceIdentifier? serviceGroupId = recoveryPlanId?.Parent;
            return parsed.ResourceType == RecoveryMembersResource.ResourceType &&
                recoveryPlanId?.ResourceType == RecoveryPlanResource.ResourceType &&
                serviceGroupId?.ResourceType == ServiceGroupResourceType &&
                string.Equals(
                    serviceGroupId.Name,
                    serviceGroup,
                    StringComparison.OrdinalIgnoreCase) &&
                Guid.TryParse(parsed.Name, out _);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            return false;
        }
    }
}
