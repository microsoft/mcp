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
    public static readonly Option<int> ProvisionedStorageGiB = new("--provisioned-storage-gib")
    {
        Description = "The desired provisioned storage size of the share in GiB",
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
            Description = "The name of the parent file share",
            Required = true
        };

        public static readonly Option<string> ConnectionName = new($"--{ConnectionNameName}")
        {
            Description = "The name of the private endpoint connection",
            Required = true
        };

        public static readonly Option<string> Status = new($"--{StatusName}")
        {
            Description = "The approval status (Approved, Rejected, or Removed)",
            Required = true
        };

        public static readonly Option<string> Description = new($"--{DescriptionName}")
        {
            Description = "The description for the connection state",
            Required = false
        };
    }
}
