// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager.RecoveryServicesBackup;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// NEW-8 regression tests: one malformed RSV protected item must not fail the whole listing.
/// </summary>
public class RsvProtectedItemPageReaderTests
{
    private const string Sub = "00000000-0000-0000-0000-000000000001";
    private const string VaultPath = $"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.RecoveryServices/vaults/vault1";

    // Shape observed in production telemetry: a classic VM item whose resource-ID fields are empty strings.
    private const string ClassicVmWithEmptyIds = """
        {
          "id": "",
          "name": "VM;iaasvmcontainer;rg1;classic-vm",
          "type": "Microsoft.RecoveryServices/vaults/backupFabrics/protectionContainers/protectedItems",
          "properties": {
            "protectedItemType": "Microsoft.ClassicCompute/virtualMachines",
            "backupManagementType": "AzureIaasVM",
            "workloadType": "VM",
            "containerName": "iaasvmcontainer;rg1;classic-vm",
            "sourceResourceId": "",
            "virtualMachineId": "",
            "policyId": "",
            "friendlyName": "classic-vm",
            "protectionStatus": "Healthy"
          }
        }
        """;

    private const string HealthyArmVm = $$"""
        {
          "id": "{{VaultPath}}/backupFabrics/Azure/protectionContainers/IaasVMContainer;iaasvmcontainerv2;rg1;vm1/protectedItems/VM;iaasvmcontainerv2;rg1;vm1",
          "name": "VM;iaasvmcontainerv2;rg1;vm1",
          "type": "Microsoft.RecoveryServices/vaults/backupFabrics/protectionContainers/protectedItems",
          "properties": {
            "protectedItemType": "Microsoft.Compute/virtualMachines",
            "backupManagementType": "AzureIaasVM",
            "workloadType": "VM",
            "containerName": "iaasvmcontainerv2;rg1;vm1",
            "sourceResourceId": "/subscriptions/{{Sub}}/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1",
            "policyId": "{{VaultPath}}/backupPolicies/DefaultPolicy",
            "friendlyName": "vm1",
            "protectionStatus": "Healthy"
          }
        }
        """;

    // Still malformed after empty strings are removed: lastBackupTime cannot be parsed.
    private const string MalformedArmVm = $$"""
        {
          "id": "{{VaultPath}}/backupFabrics/Azure/protectionContainers/IaasVMContainer;iaasvmcontainerv2;rg1;vm2/protectedItems/VM;iaasvmcontainerv2;rg1;vm2",
          "name": "VM;iaasvmcontainerv2;rg1;vm2",
          "type": "Microsoft.RecoveryServices/vaults/backupFabrics/protectionContainers/protectedItems",
          "properties": {
            "protectedItemType": "Microsoft.Compute/virtualMachines",
            "backupManagementType": "AzureIaasVM",
            "workloadType": "VM",
            "containerName": "iaasvmcontainerv2;rg1;vm2",
            "sourceResourceId": "/subscriptions/{{Sub}}/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm2",
            "policyId": "{{VaultPath}}/backupPolicies/EnhancedPolicy",
            "protectionStatus": "Unhealthy",
            "lastBackupTime": "not-a-timestamp"
          }
        }
        """;

    [Fact]
    public void Sdk_FailsOnClassicVmWithEmptyIds_ReproducesTelemetry()
    {
        // Guards the premise of the fix: the SDK deserializer itself rejects this payload.
        var ex = Assert.Throws<ArgumentException>(() => ModelReaderWriter.Read<BackupProtectedItemData>(
            BinaryData.FromString(ClassicVmWithEmptyIds),
            new ModelReaderWriterOptions("J"),
            AzureResourceManagerRecoveryServicesBackupContext.Default));

        Assert.Equal("resourceId", ex.ParamName);
    }

    [Fact]
    public void ReadItem_ClassicVmWithEmptyIds_IsReadAfterRemovingEmptyStrings()
    {
        using var doc = JsonDocument.Parse(ClassicVmWithEmptyIds);

        var item = RsvProtectedItemPageReader.ReadItem(doc.RootElement, RsvBackupOperations.MapToProtectedItemInfo);

        Assert.Equal("VM;iaasvmcontainer;rg1;classic-vm", item.Name);
        Assert.Equal("rsv", item.VaultType);
        Assert.Null(item.Id);
        Assert.Equal("iaasvmcontainer;rg1;classic-vm", item.ContainerName);
        Assert.Equal("VM", item.DatasourceType);
        Assert.Equal("Healthy", item.ProtectionStatus);
        Assert.Null(item.DatasourceId);
        Assert.Null(item.PolicyName);
    }

    [Fact]
    public void ReadItem_HealthyItem_UsesFullSdkMapping()
    {
        using var doc = JsonDocument.Parse(HealthyArmVm);

        var item = RsvProtectedItemPageReader.ReadItem(doc.RootElement, RsvBackupOperations.MapToProtectedItemInfo);

        Assert.Equal("VM;iaasvmcontainerv2;rg1;vm1", item.Name);
        Assert.Equal("iaasvmcontainerv2;rg1;vm1", item.ContainerName);
        Assert.Equal($"/subscriptions/{Sub}/resourceGroups/rg1/providers/Microsoft.Compute/virtualMachines/vm1", item.DatasourceId);
        Assert.Equal("DefaultPolicy", item.PolicyName);
        Assert.Equal("Healthy", item.ProtectionStatus);
        Assert.NotNull(item.Details);
        Assert.Equal("vm1", item.Details!.FriendlyName);
    }

    [Fact]
    public void ReadItem_MalformedItem_FallsBackToRawJson()
    {
        using var doc = JsonDocument.Parse(MalformedArmVm);

        var item = RsvProtectedItemPageReader.ReadItem(doc.RootElement, RsvBackupOperations.MapToProtectedItemInfo);

        Assert.Equal("VM;iaasvmcontainerv2;rg1;vm2", item.Name);
        Assert.Equal("rsv", item.VaultType);
        Assert.Equal("iaasvmcontainerv2;rg1;vm2", item.ContainerName);
        Assert.Equal("EnhancedPolicy", item.PolicyName);
        Assert.Equal("Unhealthy", item.ProtectionStatus);
        Assert.Equal("VM", item.DatasourceType);
        Assert.EndsWith("/virtualMachines/vm2", item.DatasourceId);
        Assert.Null(item.LastBackupTime);
        Assert.Null(item.Details);
    }

    [Fact]
    public void ReadPage_MixedItems_ReturnsEveryItemInOrderWithNextLink()
    {
        var nextLink = $"https://management.azure.com{VaultPath}/backupProtectedItems?api-version=2025-02-01&$skiptoken=abc";
        var page = $$"""{ "value": [ {{HealthyArmVm}}, {{ClassicVmWithEmptyIds}}, {{MalformedArmVm}} ], "nextLink": "{{nextLink}}" }""";
        using var doc = JsonDocument.Parse(page);

        var result = RsvProtectedItemPageReader.ReadPage(doc.RootElement, RsvBackupOperations.MapToProtectedItemInfo);

        Assert.Equal(
            ["VM;iaasvmcontainerv2;rg1;vm1", "VM;iaasvmcontainer;rg1;classic-vm", "VM;iaasvmcontainerv2;rg1;vm2"],
            result.Items.Select(i => i.Name));
        Assert.Equal(nextLink, result.NextLink);
    }

    [Theory]
    [InlineData("""{ "value": [] }""")]
    [InlineData("""{ "value": [], "nextLink": "" }""")]
    [InlineData("""{ "value": [], "nextLink": null }""")]
    [InlineData("""{ }""")]
    [InlineData("""[]""")]
    public void ReadPage_EmptyOrLastPage_HasNoItemsAndNoNextLink(string json)
    {
        using var doc = JsonDocument.Parse(json);

        var result = RsvProtectedItemPageReader.ReadPage(doc.RootElement, RsvBackupOperations.MapToProtectedItemInfo);

        Assert.Empty(result.Items);
        Assert.Null(result.NextLink);
    }

    [Fact]
    public void ReadPage_SkipsNonObjectEntries()
    {
        var page = $$"""{ "value": [ null, "x", {{HealthyArmVm}} ] }""";
        using var doc = JsonDocument.Parse(page);

        var result = RsvProtectedItemPageReader.ReadPage(doc.RootElement, RsvBackupOperations.MapToProtectedItemInfo);

        Assert.Single(result.Items);
    }

    [Fact]
    public void RemoveEmptyStrings_RemovesNestedEmptyStringsOnly()
    {
        var node = JsonNode.Parse("""
            { "a": "", "b": "keep", "c": { "d": "", "e": 0, "f": [ { "g": "" }, "" ] }, "h": null }
            """)!;

        RsvProtectedItemPageReader.RemoveEmptyStrings(node);

        Assert.Equal("""{"b":"keep","c":{"e":0,"f":[{},""]},"h":null}""", node.ToJsonString());
    }
}
