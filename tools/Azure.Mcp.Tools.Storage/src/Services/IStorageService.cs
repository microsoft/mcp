// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Models;

namespace Azure.Mcp.Tools.Storage.Services;

public interface IStorageService
{
    Task<List<StorageAccountInfo>> GetAccountDetails(
        string? account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<StorageAccountResult> CreateStorageAccount(
        string account,
        string resourceGroup,
        string location,
        string subscription,
        string? sku = null,
        string? accessTier = null,
        bool? enableHierarchicalNamespace = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<BlobInfo>> GetBlobDetails(
        string account,
        string container,
        string? blob,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<ContainerInfo>> GetContainerDetails(
        string account,
        string? container,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<ContainerInfo> CreateContainer(
        string account,
        string container,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<BlobUploadResult> UploadBlob(
        string account,
        string container,
        string blob,
        string localFilePath,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
