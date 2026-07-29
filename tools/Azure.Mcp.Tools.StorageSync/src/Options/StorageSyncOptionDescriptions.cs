// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.StorageSync.Options;

/// <summary>
/// Static definitions for all Storage Sync command options.
/// Provides centralized option definitions used across commands.
/// </summary>
public static class StorageSyncOptionDescriptions
{
    /// <summary>
    /// Storage Sync Service options.
    /// </summary>
    public static class StorageSyncService
    {
        internal const string NameDescription = "The name of the storage sync service";
        internal const string TagsDescription = "Tags to assign to the service, in the format 'tag1=value1 tag2=value2'.";
    }

    /// <summary>
    /// Sync Group options.
    /// </summary>
    public static class SyncGroup
    {
        internal const string SyncGroupNameDescription = "The name of the sync group";
    }

    /// <summary>
    /// Cloud Endpoint options.
    /// </summary>
    public static class CloudEndpoint
    {
        internal const string CloudEndpointNameDescription = "The name of the cloud endpoint";
    }

    /// <summary>
    /// Server Endpoint options.
    /// </summary>
    public static class ServerEndpoint
    {
        internal const string ServerEndpointNameDescription = "The name of the server endpoint";
        internal const string CloudTieringDescription = "Enable cloud tiering on this endpoint";
        internal const string VolumeFreeSpacePercentDescription = "Volume free space percentage to maintain (1-99, default 20)";
        internal const string TierFilesOlderThanDaysDescription = "Archive files not accessed for this many days";
        internal const string LocalCacheModeDescription = "Local cache mode: DownloadNewAndModifiedFiles, UpdateLocallyCachedFiles";
    }

    /// <summary>
    /// Registered Server options.
    /// </summary>
    public static class RegisteredServer
    {
        internal const string ServerIdDescription = "The ID/name of the registered server";
    }
}
