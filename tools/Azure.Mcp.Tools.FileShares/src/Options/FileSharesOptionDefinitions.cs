// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Options;

/// <summary>
/// Static definitions for all File Shares command options.
/// Provides centralized option definitions used across commands.
/// </summary>
public static class FileSharesOptionDefinitions
{
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
    }

    /// <summary>
    /// Private Endpoint Connection options.
    /// </summary>
    public static class PrivateEndpointConnection
    {
        private const string NameName = "connection-name";
        private const string StatusName = "status";

        public static readonly Option<string> Name = new($"--{NameName}")
        {
            Description = "The name of the private endpoint connection",
            Required = true
        };

        public static readonly Option<string> Status = new($"--{StatusName}")
        {
            Description = "The approval status (Approved, Rejected, or Removed)",
            Required = true
        };
    }
}
