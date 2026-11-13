// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.ResilienceManagement;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Runs;

internal static class DrillRunSelectedResourceValidation
{
    private static readonly ResourceType ServiceGroupResourceType =
        new("Microsoft.Management/serviceGroups");

    public static bool IsSupportedResourceId(
        string resourceId,
        string serviceGroup,
        string drill)
    {
        try
        {
            var parsed = new ResourceIdentifier(resourceId);
            ResourceIdentifier? parentId = parsed.Parent;
            ResourceIdentifier? serviceGroupId = parentId?.Parent;
            bool isRecoveryResource =
                parsed.ResourceType == RecoveryMembersResource.ResourceType &&
                parentId?.ResourceType == RecoveryPlanResource.ResourceType;
            bool isDrillResource =
                parsed.ResourceType == DrillTargetResource.ResourceType &&
                parentId?.ResourceType == ResilienceManagementDrillResource.ResourceType &&
                string.Equals(
                    parentId.Name,
                    drill,
                    StringComparison.OrdinalIgnoreCase);
            return (isRecoveryResource || isDrillResource) &&
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
