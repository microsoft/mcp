// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// UX-1 regression tests: friendly names passed as --protected-item / --container must resolve to the listed RSV item.
/// </summary>
public class RsvProtectedItemMatcherTests
{
    private const string Sub = "00000000-0000-0000-0000-000000000001";
    private const string VaultPath = $"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1";

    private static ProtectedItemInfo Vm(string rg, string vm, string? containerName = "default", bool withId = true) => new(
        Id: withId ? $"{VaultPath}/backupFabrics/Azure/protectionContainers/IaasVMContainer;iaasvmcontainerv2;{rg};{vm}/protectedItems/VM;iaasvmcontainerv2;{rg};{vm}" : null,
        Name: $"VM;iaasvmcontainerv2;{rg};{vm}",
        VaultType: "rsv",
        ProtectionStatus: "Healthy",
        DatasourceType: "VM",
        DatasourceId: $"/subscriptions/{Sub}/resourceGroups/{rg}/providers/Microsoft.Compute/virtualMachines/{vm}",
        PolicyName: "DefaultPolicy",
        LastBackupTime: null,
        ContainerName: containerName == "default" ? $"iaasvmcontainerv2;{rg};{vm}" : containerName);

    private static readonly List<ProtectedItemInfo> s_items =
    [
        Vm("rg1", "vm1"),
        Vm("rg1", "vm2"),
        Vm("rg2", "vm2"),
    ];

    [Theory]
    [InlineData("vm1")]
    [InlineData("VM1")]
    [InlineData("VM;iaasvmcontainerv2;rg1;vm1")]
    public void Resolve_ItemOnly_ResolvesUniqueItem(string protectedItem)
    {
        var match = RsvProtectedItemMatcher.Resolve(s_items, protectedItem, containerHint: null, "vault1");

        Assert.Equal("VM;iaasvmcontainerv2;rg1;vm1", match.Name);
        Assert.Equal("IaasVMContainer;iaasvmcontainerv2;rg1;vm1", RsvProtectedItemMatcher.GetContainerName(match));
    }

    [Theory]
    [InlineData("IaasVMContainer;iaasvmcontainerv2;rg2;vm2")] // full name used by the API
    [InlineData("iaasvmcontainerv2;rg2;vm2")]                 // short name returned in listings
    [InlineData("rg2;vm2")]                                   // trailing part
    public void Resolve_ContainerHint_DisambiguatesFriendlyName(string containerHint)
    {
        var match = RsvProtectedItemMatcher.Resolve(s_items, "vm2", containerHint, "vault1");

        Assert.Equal("VM;iaasvmcontainerv2;rg2;vm2", match.Name);
    }

    [Fact]
    public void Resolve_VmNameAsContainerHint_ResolvesItem()
    {
        // The most common telemetry case: the VM name passed as --container.
        var match = RsvProtectedItemMatcher.Resolve(s_items, "vm1", containerHint: "vm1", "vault1");

        Assert.Equal("VM;iaasvmcontainerv2;rg1;vm1", match.Name);
    }

    [Fact]
    public void Resolve_AmbiguousFriendlyName_ThrowsValidationErrorListingCandidates()
    {
        var ex = Assert.Throws<CommandValidationException>(() =>
            RsvProtectedItemMatcher.Resolve(s_items, "vm2", containerHint: null, "vault1"));

        Assert.Contains("matches 2 items", ex.Message);
        Assert.Contains("--container 'IaasVMContainer;iaasvmcontainerv2;rg1;vm2'", ex.Message);
        Assert.Contains("--container 'IaasVMContainer;iaasvmcontainerv2;rg2;vm2'", ex.Message);
        Assert.DoesNotContain("vm2", ex.TelemetrySafeMessage);
    }

    [Theory]
    [InlineData("vm-missing", null)]
    [InlineData("vm1", "rg2")]
    public void Resolve_NoMatch_ThrowsKeyNotFound(string protectedItem, string? containerHint)
    {
        var ex = Assert.Throws<KeyNotFoundException>(() =>
            RsvProtectedItemMatcher.Resolve(s_items, protectedItem, containerHint, "vault1"));

        Assert.Contains("was not found in vault 'vault1'", ex.Message);
    }

    [Fact]
    public void GetContainerName_PrefersFullNameFromResourceId()
    {
        // Listings report IaaS VM containers in a short form that recovery point APIs reject.
        var match = RsvProtectedItemMatcher.Resolve(s_items, "vm1", containerHint: null, "vault1");

        Assert.Equal("iaasvmcontainerv2;rg1;vm1", match.ContainerName);
        Assert.Equal("IaasVMContainer;iaasvmcontainerv2;rg1;vm1", RsvProtectedItemMatcher.GetContainerName(match));
    }

    [Fact]
    public void GetContainerName_WithoutResourceId_FallsBackToListedContainerName()
    {
        var items = new List<ProtectedItemInfo> { Vm("rg1", "vm1", withId: false) };

        var match = RsvProtectedItemMatcher.Resolve(items, "vm1", containerHint: null, "vault1");

        Assert.Equal("iaasvmcontainerv2;rg1;vm1", RsvProtectedItemMatcher.GetContainerName(match));
    }

    [Fact]
    public void Resolve_MissingContainerName_UsesResourceId()
    {
        var items = new List<ProtectedItemInfo> { Vm("rg1", "vm1", containerName: null) };

        var match = RsvProtectedItemMatcher.Resolve(items, "vm1", containerHint: null, "vault1");

        Assert.Equal("IaasVMContainer;iaasvmcontainerv2;rg1;vm1", RsvProtectedItemMatcher.GetContainerName(match));
    }

    [Fact]
    public void Resolve_NoContainerAvailable_ThrowsValidationError()
    {
        var items = new List<ProtectedItemInfo> { Vm("rg1", "vm1", containerName: null, withId: false) };

        Assert.Throws<CommandValidationException>(() =>
            RsvProtectedItemMatcher.Resolve(items, "vm1", containerHint: null, "vault1"));
    }

    [Fact]
    public void FindCandidates_ExactInternalNameWinsOverFriendlyMatches()
    {
        var exact = Vm("rg1", "vm1") with { Name = "vm1" };
        var items = new List<ProtectedItemInfo> { exact, Vm("rg2", "vm1") };

        var candidates = RsvProtectedItemMatcher.FindCandidates(items, "vm1", containerHint: null);

        Assert.Same(exact, Assert.Single(candidates));
    }

    [Theory]
    [InlineData("vm1", "IaasVMContainer;iaasvmcontainerv2;rg1;vm1", true)]
    [InlineData("VM;iaasvmcontainerv2;rg1;vm1", "vm1", true)]
    [InlineData("VM;iaasvmcontainerv2;rg1;vm1", null, true)]
    [InlineData("VM;iaasvmcontainerv2;rg1;vm1", "IaasVMContainer;iaasvmcontainerv2;rg1;vm1", false)]
    public void RequiresResolution_OnlyWhenANameIsNotQualified(string protectedItem, string? container, bool expected)
    {
        Assert.Equal(expected, RsvProtectedItemMatcher.RequiresResolution(protectedItem, container));
    }

    [Theory]
    [InlineData("BMSUserErrorContainerNameIncorrectFormat", true)]
    [InlineData("BMSUserErrorProtectedItemNameIncorrectFormat", true)]
    [InlineData("BMSUserErrorDataSourceObjectNotFound", true)]
    [InlineData("AuthorizationFailed", false)]
    [InlineData(null, false)]
    public void IsItemLookupFailure_MatchesOnlyNameErrors(string? errorCode, bool expected)
    {
        var ex = new RequestFailedException(400, "error", errorCode, null);

        Assert.Equal(expected, RsvProtectedItemMatcher.IsItemLookupFailure(ex));
    }
}
