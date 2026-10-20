// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.ResilienceManagement;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills;

internal static class DrillActionValidation
{
    private static readonly ResourceType ServiceGroupResourceType = new("Microsoft.Management/serviceGroups");

    public static void ValidateSelectedResourceIds(IReadOnlyList<string>? resourceIds, string serviceGroup, ValidationResult validationResult, string? drill = null)
    {
        if (resourceIds is not null && (resourceIds.Count == 0 ||
            resourceIds.Any(id => string.IsNullOrWhiteSpace(id) || !IsSelectedResourceId(id, serviceGroup, drill))))
        {
            var expectedResource = drill is null
                ? "recovery-resource ID under the requested service group"
                : "drill-resource ID under the requested service group and drill";
            validationResult.Errors.Add($"Selected resource IDs must contain at least one full {expectedResource} with a GUID resource name.");
        }
    }

    public static bool IsDrillResourceId(string resourceId, string serviceGroup, string drill)
        => !string.IsNullOrWhiteSpace(resourceId) && IsSelectedResourceId(resourceId, serviceGroup, drill);

    private static bool IsSelectedResourceId(string resourceId, string serviceGroup, string? drill)
    {
        try
        {
            var parsed = new ResourceIdentifier(resourceId);
            ResourceIdentifier? parentId = parsed.Parent;
            ResourceIdentifier? serviceGroupId = parentId?.Parent;
            return parsed.ResourceType == (drill is null ? RecoveryMembersResource.ResourceType : DrillTargetResource.ResourceType) &&
                parentId?.ResourceType == (drill is null ? RecoveryPlanResource.ResourceType : ResilienceManagementDrillResource.ResourceType) &&
                (drill is null || string.Equals(parentId.Name, drill, StringComparison.OrdinalIgnoreCase)) &&
                serviceGroupId?.ResourceType == ServiceGroupResourceType &&
                string.Equals(serviceGroupId.Name, serviceGroup, StringComparison.OrdinalIgnoreCase) &&
                Guid.TryParse(parsed.Name, out _);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            return false;
        }
    }

    private static readonly HashSet<string> s_modes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Failover",
        "TestFailover"
    };

    private static readonly HashSet<string> s_attestations = new(StringComparer.OrdinalIgnoreCase)
    {
        "Success",
        "Failed"
    };

    public static void ValidateResourceNames(string serviceGroup, string drill, ValidationResult validationResult)
    {
        if (serviceGroup.Length is < 1 or > 90 || !serviceGroup.All(IsValidServiceGroupNameCharacter))
        {
            validationResult.Errors.Add("The service group name must be 1 to 90 characters and contain only ASCII letters, numbers, hyphens, underscores, periods, or parentheses.");
        }

        if (drill.Length is < 1 or > 260 || !drill.All(IsValidDrillNameCharacter))
        {
            validationResult.Errors.Add("The drill name must be 1 to 260 characters and contain only ASCII letters, numbers, hyphens, underscores, or periods.");
        }
    }

    public static void ValidateMode(string mode, ValidationResult validationResult)
    {
        if (!s_modes.Contains(mode))
        {
            validationResult.Errors.Add("The drill mode must be either Failover or TestFailover.");
        }
    }

    public static void ValidateAttestation(string attestation, ValidationResult validationResult)
    {
        if (!s_attestations.Contains(attestation))
        {
            validationResult.Errors.Add("The drill attestation must be either Success or Failed.");
        }
    }

    public static string NormalizeMode(string mode) =>
        mode.Equals("Failover", StringComparison.OrdinalIgnoreCase) ? "Failover" : "TestFailover";

    public static string NormalizeAttestation(string attestation) =>
        attestation.Equals("Success", StringComparison.OrdinalIgnoreCase) ? "Success" : "Failed";

    private static bool IsValidDrillNameCharacter(char character) =>
        character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_' or '.';

    private static bool IsValidServiceGroupNameCharacter(char character) =>
        IsValidDrillNameCharacter(character) || character is '(' or ')';
}
