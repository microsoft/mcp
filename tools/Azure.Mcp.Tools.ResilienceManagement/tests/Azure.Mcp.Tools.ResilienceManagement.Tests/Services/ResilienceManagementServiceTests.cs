// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.ResilienceManagement.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

public sealed class ResilienceManagementServiceTests
{
    private const string UserAssignedIdentityResourceId = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/uami";

    [Fact]
    public void CreateRecoveryGroupsSetting_ForNewPlan_GeneratesDefaultGroupId()
    {
        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(null, null);

        Assert.True(Guid.TryParse(result.DefaultGroup.Properties?.GroupUniqueId, out _));
        Assert.Equal("Default recovery group", result.DefaultGroup.Properties?.Description);
        Assert.Empty(result.AdditionalGroups);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_ForUpdate_PreservesExistingGroups()
    {
        var existingDefaultGroup = CreateGroup("7f35c9f5-bec2-455d-8161-c904b2532e5d", 0, "Existing default group");
        var firstAdditionalGroup = CreateGroup("ddcfddaf-d15d-44fe-8472-0f3ee9f0179d", 1, "First additional group");
        var secondAdditionalGroup = CreateGroup("9db5bb96-68ab-443d-87b5-2a2555bf46e8", 2, "Second additional group");
        var existingGroups = new RecoveryGroupsSetting(existingDefaultGroup);
        existingGroups.AdditionalGroups.Add(firstAdditionalGroup);
        existingGroups.AdditionalGroups.Add(secondAdditionalGroup);

        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(existingGroups, null);

        Assert.Equal(existingDefaultGroup.Properties?.GroupUniqueId, result.DefaultGroup.Properties?.GroupUniqueId);
        Assert.Equal(existingDefaultGroup.Properties?.Description, result.DefaultGroup.Properties?.Description);
        Assert.Equal([firstAdditionalGroup, secondAdditionalGroup], result.AdditionalGroups);
    }

    [Fact]
    public void CreateRecoveryGroupsSetting_ForUpdate_OverridesDefaultGroupDescription()
    {
        var existingGroups = new RecoveryGroupsSetting(
            CreateGroup("7f35c9f5-bec2-455d-8161-c904b2532e5d", 0, "Existing default group"));

        RecoveryGroupsSetting result = ResilienceManagementService.CreateRecoveryGroupsSetting(existingGroups, "Updated default group");

        Assert.Equal("7f35c9f5-bec2-455d-8161-c904b2532e5d", result.DefaultGroup.Properties?.GroupUniqueId);
        Assert.Equal("Updated default group", result.DefaultGroup.Properties?.Description);
    }

    [Fact]
    public void CreateRecoveryPlanIdentity_UsesUserAssignedIdentity()
    {
        ManagedServiceIdentity result = ResilienceManagementService.CreateRecoveryPlanIdentity(UserAssignedIdentityResourceId);

        Assert.Equal(ManagedServiceIdentityType.UserAssigned, result.ManagedServiceIdentityType);
        Assert.Contains(new ResourceIdentifier(UserAssignedIdentityResourceId), result.UserAssignedIdentities.Keys);
    }

    [Fact]
    public void CreateRecoveryPlanIdentity_UsesSystemAssignedIdentityWhenResourceIdIsNull()
    {
        ManagedServiceIdentity result = ResilienceManagementService.CreateRecoveryPlanIdentity(null);

        Assert.Equal(ManagedServiceIdentityType.SystemAssigned, result.ManagedServiceIdentityType);
        Assert.Empty(result.UserAssignedIdentities);
    }

    [Theory]
    [InlineData("not-a-resource-id")]
    [InlineData("/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/account")]
    public void ParseUserAssignedIdentityResourceId_RejectsInvalidResourceId(string resourceId)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => ResilienceManagementService.ParseUserAssignedIdentityResourceId(resourceId));

        Assert.Contains("Microsoft.ManagedIdentity/userAssignedIdentities", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static RecoveryGroup CreateGroup(string groupId, int sequenceNumber, string description)
        => new()
        {
            Properties = new RecoveryGroupProperties(groupId, sequenceNumber, description)
        };
}
