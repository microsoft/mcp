// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Models;

namespace Azure.Mcp.Tools.ManagedLustre.Services;

public interface IManagedLustreService
{
    Task<List<LustreFileSystem>> ListFileSystemsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<int> GetRequiredAmlFSSubnetsSize(string subscription,
        string sku, int size,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<bool> CheckAmlFSSubnetAsync(
        string subscription,
        string sku,
        int size,
        string subnetId,
        string location,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<ManagedLustreSkuInfo>> SkuGetInfoAsync(
        string subscription,
        string? tenant = null,
        string? location = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<LustreFileSystem> CreateFileSystemAsync(
        string subscription,
        string resourceGroup,
        string name,
        string location,
        string sku,
        int sizeTiB,
        string subnetId,
        string zone,
        // Maintenance window
        string maintenanceDay,
        string maintenanceTime,
        // HSM
        string? hsmContainer = null,
        string? hsmLogContainer = null,
        string? importPrefix = null,
        // Root squash
        string? rootSquashMode = null,
        string? noSquashNidLists = null,
        long? squashUid = null,
        long? squashGid = null,
        // Encryption
        bool enableCustomEncryption = false,
        string? keyUrl = null,
        string? sourceVaultId = null,
        string? userAssignedIdentityId = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<LustreFileSystem> UpdateFileSystemAsync(
        string subscription,
        string resourceGroup,
        string name,
        // Maintenance window (optional)
        string? maintenanceDay = null,
        string? maintenanceTime = null,
        // Root squash updates (all optional; if UID/GID provided, both required)
        string? rootSquashMode = null,
        string? noSquashNidLists = null,
        long? squashUid = null,
        long? squashGid = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<string> CreateAutoexportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task CancelAutoexportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<Models.AutoexportJob> GetAutoexportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<Models.AutoexportJob>> ListAutoexportJobsAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task DeleteAutoexportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<string> CreateAutoimportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string? jobName = null,
        string? conflictResolutionMode = null,
        List<string>? autoimportPrefixes = null,
        string? adminStatus = null,
        bool? enableDeletions = null,
        long? maximumErrors = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task CancelAutoimportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<Models.AutoimportJob> GetAutoimportJobAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string jobName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<Models.AutoimportJob>> ListAutoimportJobsAsync(
        string subscription,
        string resourceGroup,
        string filesystemName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}

