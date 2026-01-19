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
        private const string NameName = "name";
        private const string LocationName = "location";
        private const string IncomingTrafficPolicyName = "incoming-traffic-policy";
        private const string TagsName = "tags";
        private const string IdentityTypeName = "identity-type";

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

        public static readonly Option<string> IdentityType = new($"--{IdentityTypeName}")
        {
            Description = "Managed service identity type (None, SystemAssigned, UserAssigned, SystemAssigned,UserAssigned)"
        };
    }

    /// <summary>
    /// Sync Group options.
    /// </summary>
    public static class SyncGroup
    {
        private const string NameName = "sync-group-name";

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
        private const string NameName = "cloud-endpoint-name";
        private const string StorageAccountResourceIdName = "storage-account-resource-id";
        private const string AzureFileShareNameName = "azure-file-share-name";
        private const string DirectoryPathName = "directory-path";
        private const string ChangeDetectionModeName = "change-detection-mode";
        private const string PathsName = "paths";

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
            Description = "Relative path to a directory on the Azure File share for which change detection is to be performed"
        };

        public static readonly Option<string> ChangeDetectionMode = new($"--{ChangeDetectionModeName}")
        {
            Description = "Change detection mode: 'Default' (directory only) or 'Recursive' (directory and subdirectories). Applies to the directory specified in directory-path"
        };

        public static readonly Option<string[]> Paths = new($"--{PathsName}")
        {
            Description = "Array of relative paths on the Azure File share to be included in change detection. Can be files and directories",
            AllowMultipleArgumentsPerToken = true
        };
    }

    /// <summary>
    /// Server Endpoint options.
    /// </summary>
    public static class ServerEndpoint
    {
        private const string NameName = "server-endpoint-name";
        private const string ServerResourceIdName = "server-resource-id";
        private const string ServerLocalPathName = "server-local-path";
        private const string CloudTieringName = "cloud-tiering";
        private const string VolumeFreeSpacePercentName = "volume-free-space-percent";
        private const string TierFilesOlderThanDaysName = "tier-files-older-than-days";
        private const string LocalCacheModeName = "local-cache-mode";

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

        public static readonly Option<bool?> CloudTiering = new($"--{CloudTieringName}", "-ct")
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

        public static readonly Option<string> LocalCacheMode = new($"--{LocalCacheModeName}")
        {
            Description = "Local cache mode: DownloadNewAndModifiedFiles, UpdateLocallyCachedFiles"
        };
    }

    /// <summary>
    /// Registered Server options.
    /// </summary>
    public static class RegisteredServer
    {
        private const string Id = "server-id";
        private const string Name = "server-name";

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
