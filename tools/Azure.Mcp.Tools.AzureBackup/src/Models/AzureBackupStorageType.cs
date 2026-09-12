// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Storage redundancy types supported when creating a Backup vault.</summary>
public enum AzureBackupStorageType
{
    /// <summary>Store backup data with geo-redundancy.</summary>
    GeoRedundant,

    /// <summary>Store backup data with local redundancy.</summary>
    LocallyRedundant,

    /// <summary>Store backup data with zone redundancy.</summary>
    ZoneRedundant
}
