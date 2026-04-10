// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.FileShares.Options;

/// <summary>
/// Static definitions for all File Shares command options.
/// Provides centralized option definitions used across commands.
/// </summary>
public static class FileSharesOptionDefinitions
{
    /// <summary>
    /// Common Azure location option.
    /// </summary>
    public static readonly Option<string> Location = new("--location", "-l")
    {
        Description = "The Azure region/location name (e.g., eastus, westeurope)",
        Required = false
    };

    /// <summary>
    /// Provisioned storage size in GiB.
    /// </summary>
    public static readonly Option<int> ProvisionedStorageGiB = new("--provisioned-storage-in-gib")
    {
        Description = "The desired provisioned storage size of the share in GiB",
        Required = false
    };

    /// <summary>
    /// Media tier option.
    /// </summary>
    public static readonly Option<string> MediaTier = new("--media-tier")
    {
        Description = "The storage media tier (e.g., SSD)",
        Required = false
    };

    /// <summary>
    /// Redundancy option.
    /// </summary>
    public static readonly Option<string> Redundancy = new("--redundancy")
    {
        Description = "The redundancy level (e.g., Local, Zone)",
        Required = false
    };

    /// <summary>
    /// Protocol option.
    /// </summary>
    public static readonly Option<string> Protocol = new("--protocol")
    {
        Description = "The file sharing protocol (e.g., NFS)",
        Required = false
    };

    /// <summary>
    /// Provisioned IOPS option.
    /// </summary>
    public static readonly Option<int> ProvisionedIOPerSec = new("--provisioned-io-per-sec")
    {
        Description = "The provisioned IO operations per second",
        Required = false
    };

    /// <summary>
    /// Provisioned throughput option.
    /// </summary>
    public static readonly Option<int> ProvisionedThroughputMiBPerSec = new("--provisioned-throughput-mib-per-sec")
    {
        Description = "The provisioned throughput in MiB per second",
        Required = false
    };

    /// <summary>
    /// Mount name option.
    /// </summary>
    public static readonly Option<string> MountName = new("--mount-name")
    {
        Description = "The mount name of the file share as seen by end users",
        Required = false
    };

    /// <summary>
    /// Public network access option.
    /// </summary>
    public static readonly Option<string> PublicNetworkAccess = new("--public-network-access")
    {
        Description = "Public network access setting (Enabled or Disabled)",
        Required = false
    };

    /// <summary>
    /// NFS root squash option.
    /// </summary>
    public static readonly Option<string> NfsRootSquash = new("--nfs-root-squash")
    {
        Description = "NFS root squash setting (NoRootSquash, RootSquash, or AllSquash)",
        Required = false
    };

    /// <summary>
    /// Allowed subnets option.
    /// </summary>
    public static readonly Option<string> AllowedSubnets = new("--allowed-subnets")
    {
        Description = "Comma-separated list of subnet IDs allowed to access the file share",
        Required = false
    };

    /// <summary>
    /// Tags option.
    /// </summary>
    public static readonly Option<string> Tags = new("--tags")
    {
        Description = "Resource tags as JSON (e.g., {\"key1\":\"value1\",\"key2\":\"value2\"})",
        Required = false
    };

    /// <summary>
    /// File Share options.
    /// </summary>
    public static class FileShare
    {
        private const string NameName = "name";
        private const string LocationName = "location";

        public static readonly Option<string> Name = new($"--{NameName}", "-n")
        {
            Description = "The name of the file share",
            Required = true
        };

        public static readonly Option<string> Location = new($"--{LocationName}", "-l")
        {
            Description = "The Azure region/location name (e.g., EastUS, WestEurope)",
            Required = true
        };
    }

    /// <summary>
    /// Snapshot options.
    /// </summary>
    public static class Snapshot
    {
        private const string FileShareNameName = "file-share-name";
        private const string SnapshotNameName = "snapshot-name";
        private const string MetadataName = "metadata";

        public static readonly Option<string> FileShareName = new($"--{FileShareNameName}")
        {
            Description = "The name of the parent file share",
            Required = true
        };

        public static readonly Option<string> SnapshotName = new($"--{SnapshotNameName}")
        {
            Description = "The name of the snapshot",
            Required = true
        };

        public static readonly Option<string> Metadata = new($"--{MetadataName}")
        {
            Description = "Custom metadata for the snapshot as a JSON object (e.g., {\"key1\":\"value1\",\"key2\":\"value2\"})",
            Required = false
        };
    }

    /// <summary>
    /// Private Endpoint Connection options.
    /// </summary>
    public static class PrivateEndpointConnection
    {
        private const string FileShareNameName = "file-share-name";
        private const string ConnectionNameName = "connection-name";
        private const string StatusName = "status";
        private const string DescriptionName = "description";

        public static readonly Option<string> FileShareName = new($"--{FileShareNameName}")
        {
            Description = "The name of the file share",
            Required = true
        };

        public static readonly Option<string> ConnectionName = new($"--{ConnectionNameName}")
        {
            Description = "The name of the private endpoint connection",
            Required = true
        };

        public static readonly Option<string> Status = new($"--{StatusName}")
        {
            Description = "The connection status (Approved, Rejected, or Pending)",
            Required = false
        };

        public static readonly Option<string> Description = new($"--{DescriptionName}")
        {
            Description = "Description for the connection state change",
            Required = false
        };
    }

}
