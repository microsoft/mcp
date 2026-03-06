// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Models;

namespace Azure.Mcp.Tools.Compute.Services;

public interface IComputeService
{
    // Virtual Machine operations
    Task<VmInfo> GetVmAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<VmInfo>> ListVmsAsync(
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<VmInstanceView> GetVmInstanceViewAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<(VmInfo VmInfo, VmInstanceView InstanceView)> GetVmWithInstanceViewAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<VmCreateResult> CreateVmAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string location,
        string adminUsername,
        string? vmSize = null,
        string? image = null,
        string? adminPassword = null,
        string? sshPublicKey = null,
        string? osType = null,
        string? virtualNetwork = null,
        string? subnet = null,
        string? publicIpAddress = null,
        string? networkSecurityGroup = null,
        bool? noPublicIp = null,
        string? sourceAddressPrefix = null,
        string? zone = null,
        int? osDiskSizeGb = null,
        string? osDiskType = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    // Virtual Machine Scale Set operations
    Task<VmssInfo> GetVmssAsync(
        string vmssName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<VmssInfo>> ListVmssAsync(
        string? resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<VmssVmInfo>> ListVmssVmsAsync(
        string vmssName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<VmssVmInfo> GetVmssVmAsync(
        string vmssName,
        string instanceId,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<VmssCreateResult> CreateVmssAsync(
        string vmssName,
        string resourceGroup,
        string subscription,
        string location,
        string adminUsername,
        string? vmSize = null,
        string? image = null,
        string? adminPassword = null,
        string? sshPublicKey = null,
        string? osType = null,
        string? virtualNetwork = null,
        string? subnet = null,
        int? instanceCount = null,
        string? upgradePolicy = null,
        string? zone = null,
        int? osDiskSizeGb = null,
        string? osDiskType = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<VmssUpdateResult> UpdateVmssAsync(
        string vmssName,
        string resourceGroup,
        string subscription,
        string? vmSize = null,
        int? capacity = null,
        string? upgradePolicy = null,
        bool? overprovision = null,
        bool? enableAutoOsUpgrade = null,
        string? scaleInPolicy = null,
        string? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<VmUpdateResult> UpdateVmAsync(
        string vmName,
        string resourceGroup,
        string subscription,
        string? vmSize = null,
        string? tags = null,
        string? licenseType = null,
        string? bootDiagnostics = null,
        string? userData = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    // Disk operations
    Task<DiskInfo> GetDiskAsync(
        string diskName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<DiskInfo>> ListDisksAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<DiskInfo> CreateDiskAsync(
        string diskName,
        string resourceGroup,
        string subscription,
        string? source = null,
        string? location = null,
        int? sizeGb = null,
        string? sku = null,
        string? osType = null,
        string? zone = null,
        string? hyperVGeneration = null,
        int? maxShares = null,
        string? networkAccessPolicy = null,
        string? enableBursting = null,
        string? tags = null,
        string? diskEncryptionSet = null,
        string? encryptionType = null,
        string? diskAccessId = null,
        string? tier = null,
        string? galleryImageReference = null,
        int? galleryImageReferenceLun = null,
        long? diskIopsReadWrite = null,
        long? diskMbpsReadWrite = null,
        string? uploadType = null,
        long? uploadSizeBytes = null,
        string? securityType = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<DiskInfo> UpdateDiskAsync(
        string diskName,
        string resourceGroup,
        string subscription,
        int? sizeGb = null,
        string? sku = null,
        long? diskIopsReadWrite = null,
        long? diskMbpsReadWrite = null,
        int? maxShares = null,
        string? networkAccessPolicy = null,
        string? enableBursting = null,
        string? tags = null,
        string? diskEncryptionSet = null,
        string? encryptionType = null,
        string? diskAccessId = null,
        string? tier = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
