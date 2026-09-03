// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Core;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed record ChaosRemediationTarget(
    Guid RecommendationTypeId,
    string ResourceId,
    Guid SubscriptionId,
    string ResourceGroup,
    string Vmss)
{
    private static readonly ResourceType VmssResourceType =
        new("Microsoft.Compute/virtualMachineScaleSets");

    public static bool TryCreate(
        Guid recommendationTypeId,
        string? resource,
        [NotNullWhen(true)] out ChaosRemediationTarget? target,
        out string? error)
    {
        target = null;
        error = null;

        if (recommendationTypeId == Guid.Empty)
        {
            error = "The recommendation type ID must be a non-empty GUID.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(resource) ||
            resource.Length > 2048 ||
            resource.Contains('?', StringComparison.Ordinal) ||
            resource.Contains('#', StringComparison.Ordinal) ||
            resource.Contains('\\', StringComparison.Ordinal) ||
            resource.Contains('%', StringComparison.Ordinal) ||
            resource.Any(char.IsControl))
        {
            error = "The resource must be a valid VMSS ARM resource ID.";
            return false;
        }

        ResourceIdentifier identifier;
        try
        {
            identifier = new ResourceIdentifier(resource.Trim());
        }
        catch (ArgumentException)
        {
            error = "The resource must be a valid ARM resource ID.";
            return false;
        }

        if (!identifier.ResourceType.Equals(VmssResourceType) ||
            !Guid.TryParse(identifier.SubscriptionId, out var subscriptionId) ||
            !IsValidSegment(identifier.ResourceGroupName) ||
            !IsValidSegment(identifier.Name))
        {
            error = "The resource must identify exactly one Microsoft.Compute/virtualMachineScaleSets resource.";
            return false;
        }

        target = new(
            recommendationTypeId,
            $"/subscriptions/{subscriptionId:D}/resourceGroups/{identifier.ResourceGroupName}" +
                $"/providers/Microsoft.Compute/virtualMachineScaleSets/{identifier.Name}",
            subscriptionId,
            identifier.ResourceGroupName,
            identifier.Name);
        return true;
    }

    private static bool IsValidSegment([NotNullWhen(true)] string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value is not "." and not "..";
}
