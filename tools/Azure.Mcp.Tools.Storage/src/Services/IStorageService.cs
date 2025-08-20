// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Storage.Blobs.Models;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Storage.Models;

namespace Azure.Mcp.Tools.Storage.Services;

public interface IStorageService
{
    Task<List<StorageAccountInfo>> GetStorageAccounts(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<StorageAccountInfo> GetStorageAccountDetails(
        string accountName,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<StorageAccountInfo> CreateStorageAccount(
        string accountName,
        string resourceGroup,
        string location,
        string subscription,
        string? sku = null,
        string? kind = null,
        string? accessTier = null,
        bool? enableHttpsTrafficOnly = null,
        bool? allowBlobPublicAccess = null,
        bool? enableHierarchicalNamespace = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListContainers(
        string accountName,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListTables(
        string accountName,
        string subscription,
        AuthMethod authMethod = AuthMethod.Credential,
        string? connectionString = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<string>> ListBlobs(string accountName,
        string containerName,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<BlobProperties> GetBlobDetails(
        string accountName,
        string containerName,
        string blobName,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<BlobContainerProperties> GetContainerDetails(
        string accountName,
        string containerName,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<BlobContainerProperties> CreateContainer(
        string accountName,
        string containerName,
        string subscription,
        string? blobContainerPublicAccess = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<DataLakePathInfo>> ListDataLakePaths(
        string accountName,
        string fileSystemName,
        bool recursive,
        string subscription,
        string? filterPath = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<DataLakePathInfo> CreateDirectory(
        string accountName,
        string directoryPath,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<(List<string> SuccessfulBlobs, List<string> FailedBlobs)> SetBlobTierBatch(
        string accountName,
        string containerName,
        string tier,
        string[] blobNames,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<FileShareItemInfo>> ListFilesAndDirectories(
        string accountName,
        string shareName,
        string directoryPath,
        string? prefix,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<QueueMessageSendResult> SendQueueMessage(
        string accountName,
        string queueName,
        string messageContent,
        int? timeToLiveInSeconds,
        int? visibilityTimeoutInSeconds,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
