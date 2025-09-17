// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Models;

namespace Azure.Mcp.Tools.Storage.Services;

public interface IStorageService
{
    Task<List<AccountInfo>> GetAccountDetails(
        string? account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<AccountInfo> CreateStorageAccount(
        string account,
        string resourceGroup,
        string location,
        string subscription,
        string? sku = null,
        string? accessTier = null,
        bool? enableHierarchicalNamespace = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListTables(
        string account,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? connectionString = null,
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

    Task<List<DataLakePathInfo>> ListDataLakePaths(
        string account,
        string fileSystem,
        bool recursive,
        string subscription,
        string? filterPath = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<DataLakePathInfo> CreateDirectory(
        string account,
        string directoryPath,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<(List<string> SuccessfulBlobs, List<string> FailedBlobs)> SetBlobTierBatch(
        string account,
        string container,
        string tier,
        string[] blobs,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<FileShareItemInfo>> ListFilesAndDirectories(
        string account,
        string share,
        string directoryPath,
        string? prefix,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<QueueMessageSendResult> SendQueueMessage(
        string account,
        string queue,
        string message,
        int? timeToLiveInSeconds,
        int? visibilityTimeoutInSeconds,
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
