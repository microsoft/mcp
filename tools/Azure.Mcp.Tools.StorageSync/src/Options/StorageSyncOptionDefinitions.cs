// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Static definitions for all Storage Sync command options.
/// Provides centralized option definitions used across commands.
/// </summary>
public static class StorageSyncOptionDefinitions
{
    /// <summary>
    /// Storage Sync Service options.
    /// </summary>
    public static class StorageSyncService
    {
        public const string NameName = "name";
        public const string LocationName = "location";
        public const string IncomingTrafficPolicyName = "incoming-traffic-policy";
        public const string TagsName = "tags";

        public static readonly Option<string> Name = new($"--{NameName}", "-n")
        {
            Description = "The name of the storage sync service",
            Required = true
        };

        public static readonly Option<string> Location = new($"--{LocationName}", "-l")
        {
            Description = "The Azure region/location name (e.g., EastUS, WestEurope)",
            Required = true
        };

        public static readonly Option<string> IncomingTrafficPolicy = new($"--{IncomingTrafficPolicyName}")
        {
            Description = "Incoming traffic policy for the service (AllowAllTraffic or AllowVirtualNetworksOnly)"
        };

        public static readonly Option<string> Tags = new($"--{TagsName}")
        {
            Description = "Tags to assign to the service (space-separated key=value pairs)"
        };
    }

    /// <summary>
    /// Sync Group options.
    /// </summary>
    public static class SyncGroup
    {
        public const string NameName = "sync-group-name";

        public static readonly Option<string> Name = new($"--{NameName}", "-sg")
        {
            Description = "The name of the sync group",
            Required = true
        };
    }

    /// <summary>
    /// Cloud Endpoint options.
    /// </summary>
    public static class CloudEndpoint
    {
        public const string NameName = "cloud-endpoint-name";
        public const string StorageAccountResourceIdName = "storage-account-resource-id";
        public const string AzureFileShareNameName = "azure-file-share-name";
        public const string DirectoryPathName = "directory-path";
        public const string RecursiveName = "recursive";

        public static readonly Option<string> Name = new($"--{NameName}", "-ce")
        {
            Description = "The name of the cloud endpoint",
            Required = true
        };

        public static readonly Option<string> StorageAccountResourceId = new($"--{StorageAccountResourceIdName}")
        {
            Description = "The resource ID of the Azure storage account",
            Required = true
        };

        public static readonly Option<string> AzureFileShareName = new($"--{AzureFileShareNameName}")
        {
            Description = "The name of the Azure file share",
            Required = true
        };

        public static readonly Option<string> DirectoryPath = new($"--{DirectoryPathName}")
        {
            Description = "The directory path for change detection"
        };

        public static readonly Option<bool> Recursive = new($"--{RecursiveName}", "-r")
        {
            Description = "Recursively include subdirectories for change detection"
        };
    }

    /// <summary>
    /// Server Endpoint options.
    /// </summary>
    public static class ServerEndpoint
    {
        public const string NameName = "server-endpoint-name";
        public const string ServerResourceIdName = "server-resource-id";
        public const string ServerLocalPathName = "server-local-path";
        public const string CloudTieringName = "cloud-tiering";
        public const string VolumeFreeSpacePercentName = "volume-free-space-percent";
        public const string TierFilesOlderThanDaysName = "tier-files-older-than-days";

        public static readonly Option<string> Name = new($"--{NameName}", "-se")
        {
            Description = "The name of the server endpoint",
            Required = true
        };

        public static readonly Option<string> ServerResourceId = new($"--{ServerResourceIdName}")
        {
            Description = "The resource ID of the registered server",
            Required = true
        };

        public static readonly Option<string> ServerLocalPath = new($"--{ServerLocalPathName}")
        {
            Description = "The local folder path on the server for syncing",
            Required = true
        };

        public static readonly Option<bool> CloudTiering = new($"--{CloudTieringName}", "-ct")
        {
            Description = "Enable cloud tiering on this endpoint"
        };

        public static readonly Option<int> VolumeFreeSpacePercent = new($"--{VolumeFreeSpacePercentName}")
        {
            Description = "Volume free space percentage to maintain (1-99, default 20)"
        };

        public static readonly Option<int> TierFilesOlderThanDays = new($"--{TierFilesOlderThanDaysName}")
        {
            Description = "Archive files not accessed for this many days"
        };
    }

    /// <summary>
    /// Registered Server options.
    /// </summary>
    public static class RegisteredServer
    {
        public const string Id = "server-id";
        public const string Name = "server-name";

        public static readonly Option<string> ServerId = new($"--{Id}")
        {
            Description = "The ID/name of the registered server",
            Required = true
        };

        public static readonly Option<string> ServerName = new($"--{Name}")
        {
            Description = "The name of the registered server",
            Required = true
        };
    }
}
