// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options;

/// <summary>
/// Static definitions for all File Shares command options.
/// Provides centralized option definitions used across commands.
/// </summary>
internal static class FileSharesOptionDescriptions
{
    internal const string Name = "The name of the file share";
    internal const string Location = "The Azure region/location name (e.g., EastUS, WestEurope)";
    internal const string ProvisionedStorageGiB = "The desired provisioned storage size of the share in GiB";
    internal const string ProvisionedIOPerSec = "The provisioned IO operations per second";
    internal const string ProvisionedThroughputMiBPerSec = "The provisioned throughput in MiB per second";
    internal const string PublicNetworkAccess = "Public network access setting (Enabled or Disabled)";
    internal const string NfsRootSquash = "NFS root squash setting (NoRootSquash, RootSquash, or AllSquash)";
    internal const string NfsEncryptionInTransit = "NFS encryption in transit setting (Enabled or Disabled)";
    internal const string AllowedSubnets = "Comma-separated list of subnet IDs allowed to access the file share";
    internal const string Tags = "Resource tags as JSON (e.g., {\"key1\":\"value1\",\"key2\":\"value2\"})";
    internal const string SnapshotFileShareName = "The name of the parent file share";
    internal const string SnapshotName = "The name of the snapshot";
    internal const string Metadata = "Custom metadata for the snapshot as a JSON object (e.g., {\"key1\":\"value1\",\"key2\":\"value2\"})";
    internal const string FileShareName = "The name of the file share";
    internal const string ConnectionName = "The name of the private endpoint connection";
}
