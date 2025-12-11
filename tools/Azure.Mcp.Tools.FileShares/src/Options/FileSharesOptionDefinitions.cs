// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        public const string NameName = "name";
        public const string LocationName = "location";
        public const string QuotaName = "quota";
        public const string AccessTierName = "access-tier";
        public const string EnableSmb3Name = "enable-smb3";
        public const string FilterName = "filter";

        public static readonly Option<string> Name = new($"--{NameName}", "-n")
        {
            Description = "The resource name of the file share",
            IsRequired = true
        };

        public static readonly Option<string> Location = new($"--{LocationName}", "-l")
        {
            Description = "The Azure region/location name (e.g., EastUS, WestEurope)",
            IsRequired = true
        };

        public static readonly Option<int> Quota = new($"--{QuotaName}", "-q")
        {
            Description = "The quota in GiB (100-102400)"
        };

        public static readonly Option<string> AccessTier = new($"--{AccessTierName}")
        {
            Description = "The access tier (Hot, Cool, or TransactionOptimized)"
        };

        public static readonly Option<bool> EnableSmb3 = new($"--{EnableSmb3Name}")
        {
            Description = "Enable SMB 3.1.1 multichannel support"
        };

        public static readonly Option<string> Filter = new($"--{FilterName}", "-f")
        {
            Description = "Optional filter expression to apply to results"
        };
    }

    /// <summary>
    /// File Share Snapshot options.
    /// </summary>
    public static class Snapshot
    {
        public const string NameName = "snapshot-name";
        public const string FileShareNameName = "file-share-name";

        public static readonly Option<string> Name = new($"--{NameName}", "-sn")
        {
            Description = "The name of the snapshot",
            IsRequired = true
        };

        public static readonly Option<string> FileShareName = new($"--{FileShareNameName}", "-fsn")
        {
            Description = "The name of the parent file share",
            IsRequired = true
        };
    }

    /// <summary>
    /// Private Endpoint Connection options.
    /// </summary>
    public static class PrivateEndpointConnection
    {
        public const string NameName = "connection-name";
        public const string StatusName = "status";
        public const string DescriptionName = "description";

        public static readonly Option<string> Name = new($"--{NameName}", "-cn")
        {
            Description = "The name of the private endpoint connection",
            IsRequired = true
        };

        public static readonly Option<string> Status = new($"--{StatusName}", "-s")
        {
            Description = "The connection status (Approved or Rejected)",
            IsRequired = false
        };

        public static readonly Option<string> Description = new($"--{DescriptionName}", "-d")
        {
            Description = "The reason for approval/rejection of the connection",
            IsRequired = false
        };
    }

    /// <summary>
    /// Storage Account option shared across commands.
    /// </summary>
    public static readonly Option<string> Account = new("--account", "-a")
    {
        Description = "The storage account name"
    };
}
