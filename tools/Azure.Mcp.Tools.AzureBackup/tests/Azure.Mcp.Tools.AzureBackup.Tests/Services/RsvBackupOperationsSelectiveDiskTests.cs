// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Services;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.Tests.Services;

/// <summary>
/// Direct unit tests for the internal selective-disk-backup translation helper
/// <see cref="RsvBackupOperations.ApplyDiskExclusionToProtectedItem"/> that maps
/// our <see cref="DiskExclusionSpec"/> onto the Azure Resource Manager SDK shape
/// (<see cref="IaasVmBackupExtendedProperties.DiskExclusionProperties"/>).
///
/// These tests execute against the exact SDK types the ARM RecoveryServicesBackup
/// client uses, so they validate the customer-visible payload without hitting Azure.
/// This is the primary "outside of live tests" coverage for the selective disk
/// backup feature - selective disk backup applies only to RSV IaaS VM protected
/// items (not SQL / SAP HANA / SAP ASE in IaaS VM, and not DPP workloads), so this
/// translation only ever runs against an <see cref="IaasComputeVmProtectedItem"/>.
/// </summary>
public class RsvBackupOperationsSelectiveDiskTests
{
    [Fact]
    public void ApplyDiskExclusionToProtectedItem_NullSpec_LeavesExtendedPropertiesUntouched()
    {
        var vm = new IaasComputeVmProtectedItem();

        RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, diskExclusion: null);

        Assert.Null(vm.ExtendedProperties);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_EmptySpec_LeavesExtendedPropertiesUntouched()
    {
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(Setting: null, DiskLunsCsv: null, ExcludeAllDataDisks: false);
        Assert.False(spec.HasAnyValue);

        RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec);

        Assert.Null(vm.ExtendedProperties);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_ResetSetting_ClearsDiskExclusionProperties()
    {
        // 'resetexclusionsettings' must produce ExtendedProperties with DiskExclusionProperties=null
        // so the ARM PUT explicitly removes any prior selective-disk configuration.
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(DiskExclusionSpec.SettingReset, DiskLunsCsv: null, ExcludeAllDataDisks: false);

        RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec);

        Assert.NotNull(vm.ExtendedProperties);
        Assert.Null(vm.ExtendedProperties!.DiskExclusionProperties);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_ExcludeAllDataDisks_ProducesEmptyInclusionList()
    {
        // --exclude-all-data-disks means "back up only the OS disk". The wire representation is
        // IsInclusionList=true with an empty DiskLunList (nothing beyond the OS disk is included).
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(Setting: null, DiskLunsCsv: null, ExcludeAllDataDisks: true);

        RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec);

        Assert.NotNull(vm.ExtendedProperties);
        var props = vm.ExtendedProperties!.DiskExclusionProperties;
        Assert.NotNull(props);
        Assert.True(props!.IsInclusionList);
        Assert.Empty(props.DiskLunList);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_ExcludeAllDataDisks_SerializesEmptyDiskLunList()
    {
        // Regression guard for the ChangeTrackingList<int> quirk: `DiskLunList` is a
        // ChangeTrackingList and is OMITTED from the JSON payload if the list has never been
        // touched. On the --exclude-all-data-disks path we must force materialization so the
        // wire body carries `"diskLunList": []`. The RSV service rejects a payload with
        // {isInclusionList:true} but no diskLunList, so removing the workaround silently
        // breaks the "back up only the OS disk" contract.
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(Setting: null, DiskLunsCsv: null, ExcludeAllDataDisks: true);

        RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec);

        var props = vm.ExtendedProperties!.DiskExclusionProperties!;
        var payload = ((IJsonModel<DiskExclusionProperties>)props).Write(ModelReaderWriterOptions.Json).ToString();

        using var doc = JsonDocument.Parse(payload);
        Assert.True(
            doc.RootElement.TryGetProperty("diskLunList", out var diskLunList),
            $"diskLunList must be emitted on the wire for --exclude-all-data-disks; payload was: {payload}");
        Assert.Equal(JsonValueKind.Array, diskLunList.ValueKind);
        Assert.Equal(0, diskLunList.GetArrayLength());
        Assert.True(doc.RootElement.GetProperty("isInclusionList").GetBoolean());
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_ExcludeAllOverridesSettingAndLuns()
    {
        // Contract: --exclude-all-data-disks is a stronger switch than --disks-list. If both are
        // supplied, "exclude all data disks" wins (matches the CLI documented contract).
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(
            Setting: DiskExclusionSpec.SettingExclude,
            DiskLunsCsv: "0,1,2",
            ExcludeAllDataDisks: true);

        RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec);

        var props = vm.ExtendedProperties!.DiskExclusionProperties!;
        Assert.True(props.IsInclusionList);
        Assert.Empty(props.DiskLunList);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_IncludeWithLuns_SetsInclusionListTrue()
    {
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(
            Setting: DiskExclusionSpec.SettingInclude,
            DiskLunsCsv: "0,2,5",
            ExcludeAllDataDisks: false);

        RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec);

        var props = vm.ExtendedProperties!.DiskExclusionProperties!;
        Assert.True(props.IsInclusionList);
        Assert.Equal(new[] { 0, 2, 5 }, props.DiskLunList);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_ExcludeWithLuns_SetsInclusionListFalse()
    {
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(
            Setting: DiskExclusionSpec.SettingExclude,
            DiskLunsCsv: "1, 3",
            ExcludeAllDataDisks: false);

        RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec);

        var props = vm.ExtendedProperties!.DiskExclusionProperties!;
        Assert.False(props.IsInclusionList);
        Assert.Equal(new[] { 1, 3 }, props.DiskLunList);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_SettingIsCaseInsensitive()
    {
        // The command layer already normalizes casing, but the translation helper is used by
        // both 'protect' and 'update-protection' and must also tolerate the raw casing.
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(
            Setting: "INCLUDE",
            DiskLunsCsv: "0",
            ExcludeAllDataDisks: false);

        RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec);

        var props = vm.ExtendedProperties!.DiskExclusionProperties!;
        Assert.True(props.IsInclusionList);
        Assert.Equal(new[] { 0 }, props.DiskLunList);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_IncludeMissingLuns_ThrowsArgumentException()
    {
        // 'include' / 'exclude' require an explicit LUN list. This mirrors the CLI contract of
        // 'az backup protection enable-for-vm --disk-list-setting include --disks-list ...'.
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(
            Setting: DiskExclusionSpec.SettingInclude,
            DiskLunsCsv: null,
            ExcludeAllDataDisks: false);

        var ex = Assert.Throws<ArgumentException>(() =>
            RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec));

        Assert.Contains("--disks-list", ex.Message);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_ExcludeWithNegativeLun_ThrowsArgumentException()
    {
        // OS disk (LUN -1) is never a user-selectable value on the wire.
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(
            Setting: DiskExclusionSpec.SettingExclude,
            DiskLunsCsv: "-1",
            ExcludeAllDataDisks: false);

        var ex = Assert.Throws<ArgumentException>(() =>
            RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec));

        Assert.Contains("non-negative", ex.Message);
    }

    [Fact]
    public void ApplyDiskExclusionToProtectedItem_ExcludeWithNonIntLun_ThrowsArgumentException()
    {
        var vm = new IaasComputeVmProtectedItem();
        var spec = new DiskExclusionSpec(
            Setting: DiskExclusionSpec.SettingExclude,
            DiskLunsCsv: "0,foo,2",
            ExcludeAllDataDisks: false);

        var ex = Assert.Throws<ArgumentException>(() =>
            RsvBackupOperations.ApplyDiskExclusionToProtectedItem(vm, spec));

        Assert.Contains("Invalid disk LUN", ex.Message);
    }
}
