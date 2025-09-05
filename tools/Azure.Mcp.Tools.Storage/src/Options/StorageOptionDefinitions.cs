// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Storage.Options;

public static class StorageOptionDefinitions
{
    public const string AccountName = "account";
    public const string AccountCreateName = "account";
    public const string ContainerName = "container";
    public const string TableName = "table";
    public const string FileSystemName = "file-system";
    public const string DirectoryPathName = "directory-path";
    public const string TierName = "tier";
    public const string BlobName = "blob";
    public const string BlobsName = "blobs";
    public const string FilterPathName = "filter-path";
    public const string RecursiveName = "recursive";
    public const string ShareName = "share";
    public const string PrefixName = "prefix";
    public const string QueueName = "queue";
    public const string MessageName = "message";
    public const string TimeToLiveInSecondsName = "time-to-live-in-seconds";
    public const string VisibilityTimeoutInSecondsName = "visibility-timeout-in-seconds";
    public const string LocalFilePathName = "local-file-path";
    public const string LocationName = "location";
    public const string SkuName = "sku";
    public const string AccessTierName = "access-tier";
    public const string EnableHierarchicalNamespaceName = "enable-hierarchical-namespace";

    public static readonly Option<string> Account = new(
        $"--{AccountName}"
    )
    {
        Description = "The name of the Azure Storage account. This is the unique name you chose for your storage account (e.g., 'mystorageaccount').",
        Required = true
    };

    public static readonly Option<string> AccountCreate = new(
        $"--{AccountCreateName}"
    )
    {
        Description = "The name of the Azure Storage account to create. Must be globally unique, 3-24 characters, lowercase letters and numbers only.",
        Required = true
    };

    public static readonly Option<string> Location = new(
        $"--{LocationName}"
    )
    {
        Description = "The Azure region where the storage account will be created (e.g., 'eastus', 'westus2').",
        Required = true
    };

    public static readonly Option<string> Sku = new(
        $"--{SkuName}"
    )
    {
        Description = "The storage account SKU. Valid values: Standard_LRS, Standard_GRS, Standard_RAGRS, Standard_ZRS, Premium_LRS, Premium_ZRS, Standard_GZRS, Standard_RAGZRS.",
        Required = false
    };

    public static readonly Option<string> AccessTier = new(
        $"--{AccessTierName}"
    )
    {
        Description = "The default access tier for blob storage. Valid values: Hot, Cool.",
        Required = false
    };

    public static readonly Option<bool> EnableHierarchicalNamespace = new($"--{EnableHierarchicalNamespaceName}")
    {
        Description = "Whether to enable hierarchical namespace (Data Lake Storage Gen2) for the storage account.",
        DefaultValueFactory = _ => false,
        Required = false
    };

    public static readonly Option<string> Container = new(
        $"--{ContainerName}"
    )
    {
        Description = "The name of the container to access within the storage account.",
        Required = true
    };

    public static readonly Option<string> Table = new(
        $"--{TableName}"
    )
    {
        Description = "The name of the table to access within the storage account.",
        Required = true
    };

    public static readonly Option<string> FileSystem = new(
        $"--{FileSystemName}"
    )
    {
        Description = "The name of the Data Lake file system to access within the storage account.",
        Required = true
    };

    public static readonly Option<string> DirectoryPath = new(
        $"--{DirectoryPathName}"
    )
    {
        Description = "The full path of the directory to create in the Data Lake, including the file system name (e.g., 'myfilesystem/data/logs' or 'myfilesystem/archives/2024'). Use forward slashes (/) to separate the file system name from the directory path and for subdirectories.",
        Required = true
    };

    public static readonly Option<string> Tier = new(
        $"--{TierName}"
    )
    {
        Description = "The access tier to set for the blobs. Valid values include Hot, Cool, Archive, and others depending on the storage account type. See Azure documentation for the complete list of supported access tiers.",
        Required = true
    };

    public static readonly Option<string[]> Blobs = new(
        $"--{BlobsName}"
    )
    {
        Description = "The names of the blobs to set the access tier for. Provide multiple blob names separated by spaces. Each blob name should be the full path within the container (e.g., 'file1.txt' or 'folder/file2.txt').",
        Required = true,
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<string> Blob = new(
        $"--{BlobName}"
    )
    {
        Description = "The name of the blob to access within the container. This should be the full path within the container (e.g., 'file.txt' or 'folder/file.txt').",
        Required = true
    };

    public static readonly Option<string> FilterPath = new(
        $"--{FilterPathName}"
    )
    {
        Description = "The prefix to filter paths in the Data Lake. Only paths that start with this prefix will be listed.",
        Required = false
    };

    public static readonly Option<bool> Recursive = new($"--{RecursiveName}")
    {
        Description = "Flag to indicate whether the command will operate recursively on all subdirectories.",
        DefaultValueFactory = _ => false,
        Required = false
    };

    public static readonly Option<string> Share = new(
        $"--{ShareName}"
    )
    {
        Description = "The name of the file share to access within the storage account.",
        Required = true
    };

    public static readonly Option<string> Prefix = new(
        $"--{PrefixName}"
    )
    {
        Description = "Optional prefix to filter results. Only items that start with this prefix will be returned.",
        Required = false
    };

    public static readonly Option<string> Queue = new(
        $"--{QueueName}"
    )
    {
        Description = "The name of the queue to access within the storage account.",
        Required = true
    };

    public static readonly Option<string> Message = new(
        $"--{MessageName}"
    )
    {
        Description = "The content of the message to send to the queue.",
        Required = true
    };

    public static readonly Option<int?> TimeToLiveInSeconds = new(
        $"--{TimeToLiveInSecondsName}"
    )
    {
        Description = "The time-to-live for the message in seconds. If not specified, the message will use the queue's default TTL. Set to -1 for messages that never expire.",
        Required = false
    };

    public static readonly Option<int?> VisibilityTimeoutInSeconds = new(
        $"--{VisibilityTimeoutInSecondsName}"
    )
    {
        Description = "The visibility timeout for the message in seconds. This determines how long the message will be invisible after it's retrieved. If not specified, defaults to 0 (immediately visible).",
        Required = false
    };

    public static readonly Option<string> LocalFilePath = new(
        $"--{LocalFilePathName}"
    )
    {
        Description = "The local file path to read content from or to write content to. This should be the full path to the file on your local system.",
        Required = true
    };
}
