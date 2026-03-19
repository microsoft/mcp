// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Central registry of all DPP (Data Protection / Backup Vault) datasource profiles.
/// Acts as a single source of truth for datasource-specific configuration, replacing
/// scattered if/else checks throughout DppBackupOperations.
///
/// To add a new DPP datasource type:
///   1. Add a static DppDatasourceProfile instance below
///   2. Register it in the AllProfiles array
///   — No other code changes needed in DppBackupOperations.
/// </summary>
public static class DppDatasourceRegistry
{

    public static readonly DppDatasourceProfile AzureDisk = new()
    {
        FriendlyName = "AzureDisk",
        ArmResourceType = "Microsoft.Compute/disks",
        Aliases = ["azuredisk", "disk"],
        UsesOperationalStore = true,
        ScheduleInterval = "PT4H",
        BackupType = "Incremental",
        BackupRuleName = "BackupHourly",
        DefaultRetentionDays = 7,
        RequiresSnapshotResourceGroup = true,
        DefaultRestoreMode = DppRestoreMode.RecoveryPoint,
        SupportsPolicyUpdate = false,
    };

    public static readonly DppDatasourceProfile AzureBlob = new()
    {
        FriendlyName = "AzureBlob",
        ArmResourceType = "Microsoft.Storage/storageAccounts/blobServices",
        Aliases = ["azureblob", "blob"],
        UsesOperationalStore = true,
        IsContinuousBackup = true,
        DefaultRetentionDays = 30,
        RequiresSnapshotResourceGroup = false,
        DefaultRestoreMode = DppRestoreMode.PointInTime,
        AutoDetectFromBaseResourceType = "Microsoft.Storage/storageAccounts",
        SupportsPolicyUpdate = false,
    };

    public static readonly DppDatasourceProfile Aks = new()
    {
        FriendlyName = "AKS",
        ArmResourceType = "Microsoft.ContainerService/managedClusters",
        Aliases = ["aks", "kubernetes"],
        UsesOperationalStore = true,
        ScheduleInterval = "PT4H",
        BackupType = "Incremental",
        BackupRuleName = "BackupHourly",
        DefaultRetentionDays = 7,
        RequiresSnapshotResourceGroup = true,
        DataSourceSetMode = DppDataSourceSetMode.Self,
        BackupParametersMode = DppBackupParametersMode.KubernetesCluster,
        DefaultRestoreMode = DppRestoreMode.RecoveryPoint,
        SupportsPolicyUpdate = false,
    };

    public static readonly DppDatasourceProfile ElasticSan = new()
    {
        FriendlyName = "ElasticSAN",
        ArmResourceType = "Microsoft.ElasticSan/elasticSans/volumeGroups",
        Aliases = ["elasticsan", "esan"],
        UsesOperationalStore = true,
        ScheduleInterval = "P1D",
        BackupType = "Incremental",
        BackupRuleName = "BackupDaily",
        DefaultRetentionDays = 7,
        RequiresSnapshotResourceGroup = true,
        DataSourceSetMode = DppDataSourceSetMode.Parent,
        InstanceNamingMode = DppInstanceNamingMode.ParentChild,
        DefaultRestoreMode = DppRestoreMode.RecoveryPoint,
        SupportsPolicyUpdate = false,
    };

    public static readonly DppDatasourceProfile PostgreSqlFlexible = new()
    {
        FriendlyName = "PostgreSQLFlexible",
        ArmResourceType = "Microsoft.DBforPostgreSQL/flexibleServers",
        Aliases = ["postgresqlflexible", "pgflex", "postgresql"],
        UsesOperationalStore = false,
        ScheduleInterval = "P1D",
        BackupType = "Full",
        BackupRuleName = "BackupDaily",
        DefaultRetentionDays = 30,
        RequiresSnapshotResourceGroup = false,
        DefaultRestoreMode = DppRestoreMode.RestoreAsFiles,
        SupportsPolicyUpdate = false,
    };

    public static readonly DppDatasourceProfile MySqlFlexible = new()
    {
        FriendlyName = "MySQLFlexible",
        ArmResourceType = "Microsoft.DBforMySQL/flexibleServers",
        Aliases = ["mysqlflexible", "mysql"],
        UsesOperationalStore = false,
        ScheduleInterval = "P1D",
        BackupType = "Full",
        BackupRuleName = "BackupDaily",
        DefaultRetentionDays = 30,
        RequiresSnapshotResourceGroup = false,
        DefaultRestoreMode = DppRestoreMode.RestoreAsFiles,
        SupportsPolicyUpdate = false,
    };

    public static readonly DppDatasourceProfile AzureDataLakeStorage = new()
    {
        FriendlyName = "AzureDataLakeStorage",
        ArmResourceType = "Microsoft.Storage/storageAccounts/blobServices",
        Aliases = ["adls", "datalake", "datalakestorage"],
        UsesOperationalStore = true,
        IsContinuousBackup = true,
        DefaultRetentionDays = 30,
        RequiresSnapshotResourceGroup = false,
        DefaultRestoreMode = DppRestoreMode.PointInTime,
        SupportsPolicyUpdate = false,
    };

    public static readonly DppDatasourceProfile CosmosDb = new()
    {
        FriendlyName = "CosmosDB",
        ArmResourceType = "Microsoft.DocumentDB/databaseAccounts",
        Aliases = ["cosmosdb", "cosmos"],
        UsesOperationalStore = true,
        IsContinuousBackup = true,
        DefaultRetentionDays = 30,
        RequiresSnapshotResourceGroup = false,
        DefaultRestoreMode = DppRestoreMode.PointInTime,
        SupportsPolicyUpdate = false,
    };


    /// <summary>All registered DPP datasource profiles.</summary>
    public static readonly DppDatasourceProfile[] AllProfiles =
    [
        AzureDisk,
        AzureBlob,
        Aks,
        ElasticSan,
        PostgreSqlFlexible,
        MySqlFlexible,
        AzureDataLakeStorage,
        CosmosDb,
    ];

    /// <summary>
    /// Resolves a user-supplied workload type or ARM resource type to the matching profile.
    /// Case-insensitive match against FriendlyName, Aliases, and ArmResourceType.
    /// Falls back to a generic profile if no match is found (preserves backward compatibility).
    /// </summary>
    public static DppDatasourceProfile Resolve(string workloadTypeOrArmType)
    {
        var normalised = workloadTypeOrArmType.ToLowerInvariant();

        foreach (var profile in AllProfiles)
        {
            if (normalised.Equals(profile.FriendlyName, StringComparison.OrdinalIgnoreCase))
            {
                return profile;
            }

            if (normalised.Equals(profile.ArmResourceType, StringComparison.OrdinalIgnoreCase))
            {
                return profile;
            }

            foreach (var alias in profile.Aliases)
            {
                if (normalised.Equals(alias, StringComparison.OrdinalIgnoreCase))
                {
                    return profile;
                }
            }
        }

        return new DppDatasourceProfile
        {
            FriendlyName = workloadTypeOrArmType,
            ArmResourceType = workloadTypeOrArmType,
            UsesOperationalStore = false,
            ScheduleInterval = "P1D",
            BackupType = "Full",
            BackupRuleName = "BackupDaily",
            DefaultRetentionDays = 30,
        };
    }

    /// <summary>
    /// Tries to auto-detect a profile when the user supplies a base resource type
    /// (e.g. "Microsoft.Storage/storageAccounts") that needs re-mapping to a child type
    /// (e.g. Blob → "Microsoft.Storage/storageAccounts/blobServices").
    /// </summary>
    public static DppDatasourceProfile? TryAutoDetect(string armResourceType)
    {
        var normalised = armResourceType.ToLowerInvariant();

        foreach (var profile in AllProfiles)
        {
            if (profile.AutoDetectFromBaseResourceType != null &&
                normalised.Equals(profile.AutoDetectFromBaseResourceType, StringComparison.OrdinalIgnoreCase))
            {
                return profile;
            }
        }

        return null;
    }

    /// <summary>
    /// Derives the parent resource ID from a child resource ID (e.g. ESAN volume group → parent ESAN).
    /// Generic logic that strips the last two path segments (/childType/childName).
    /// </summary>
    public static ResourceIdentifier GetParentResourceId(ResourceIdentifier childResourceId)
    {
        var idStr = childResourceId.ToString();

        var lastSlash = idStr.LastIndexOf('/');
        if (lastSlash > 0)
        {
            var secondLastSlash = idStr.LastIndexOf('/', lastSlash - 1);
            if (secondLastSlash > 0)
            {
                return new ResourceIdentifier(idStr[..secondLastSlash]);
            }
        }

        return childResourceId.Parent ?? childResourceId;
    }

    /// <summary>
    /// Generates a backup instance name based on the profile's naming mode.
    /// </summary>
    public static string GenerateInstanceName(DppDatasourceProfile profile, ResourceIdentifier datasourceResourceId)
    {
        return profile.InstanceNamingMode switch
        {
            DppInstanceNamingMode.ParentChild =>
                $"{GetParentResourceId(datasourceResourceId).Name}-{datasourceResourceId.Name}-{Guid.NewGuid()}",
            _ =>
                $"{datasourceResourceId.Name}-{datasourceResourceId.Name}-{Guid.NewGuid().ToString("N")[..12]}",
        };
    }
}
