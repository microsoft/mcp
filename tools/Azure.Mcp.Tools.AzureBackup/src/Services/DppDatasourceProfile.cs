// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Defines how a DPP datasource type builds its DataSourceSetInfo in protect/restore payloads.
/// </summary>
public enum DppDataSourceSetMode
{
    /// <summary>No DataSourceSetInfo required (Disk, Blob, PGFlex).</summary>
    None,

    /// <summary>DataSourceSetInfo points to the datasource itself (AKS).</summary>
    Self,

    /// <summary>DataSourceSetInfo points to the parent resource (ESAN volume group → parent ESAN).</summary>
    Parent,
}

/// <summary>
/// Defines the default restore approach for a DPP datasource type.
/// </summary>
public enum DppRestoreMode
{
    /// <summary>Restore using a discrete recovery point ID.</summary>
    RecoveryPoint,

    /// <summary>Restore to a specific point in time (Blob, ADLS continuous backup).</summary>
    PointInTime,

    /// <summary>Restore as files to a storage account container (PGFlex).</summary>
    RestoreAsFiles,
}

/// <summary>
/// Defines instance naming patterns for DPP backup instances.
/// </summary>
public enum DppInstanceNamingMode
{
    /// <summary>Standard naming: {resourceName}-{resourceName}-{shortGuid}.</summary>
    Standard,

    /// <summary>Parent-child naming for ESAN: {parentName}-{childName}-{guid}.</summary>
    ParentChild,
}

/// <summary>
/// Defines whether the datasource requires additional backup parameters during protection.
/// </summary>
public enum DppBackupParametersMode
{
    /// <summary>No additional backup parameters needed.</summary>
    None,

    /// <summary>AKS requires KubernetesClusterBackupDataSourceSettings.</summary>
    KubernetesCluster,
}

/// <summary>
/// Immutable profile describing all configuration aspects of a DPP datasource type.
/// Eliminates hardcoded if/else branching by centralizing datasource-specific behaviour.
/// Each property drives a specific dimension of protect, policy-create, and restore operations.
/// </summary>
/// <remarks>
/// AOT-safe: no reflection, no Func delegates — pure data record with enum-driven dispatch.
/// </remarks>
public sealed record DppDatasourceProfile
{

    /// <summary>Friendly name used for user-facing resolution (e.g. "AzureDisk", "AKS").</summary>
    public required string FriendlyName { get; init; }

    /// <summary>ARM resource type string (e.g. "Microsoft.Compute/disks").</summary>
    public required string ArmResourceType { get; init; }

    /// <summary>Alternative user-supplied names that resolve to this profile.</summary>
    public string[] Aliases { get; init; } = [];


    /// <summary>Whether this datasource uses OperationalStore (snapshot-based) or VaultStore.</summary>
    public bool UsesOperationalStore { get; init; }


    /// <summary>True for continuous backup (Blob, ADLS) — no scheduled backup rule created.</summary>
    public bool IsContinuousBackup { get; init; }

    /// <summary>ISO 8601 schedule interval (e.g. "PT4H", "P1D"). Ignored when <see cref="IsContinuousBackup"/> is true.</summary>
    public string ScheduleInterval { get; init; } = "P1D";

    /// <summary>Backup type for the backup rule (e.g. "Incremental", "Full"). Ignored when <see cref="IsContinuousBackup"/> is true.</summary>
    public string BackupType { get; init; } = "Full";

    /// <summary>Name of the backup rule (e.g. "BackupHourly", "BackupDaily"). Ignored when <see cref="IsContinuousBackup"/> is true.</summary>
    public string BackupRuleName { get; init; } = "BackupDaily";

    /// <summary>Default retention days when user doesn't specify.</summary>
    public int DefaultRetentionDays { get; init; } = 30;


    /// <summary>Whether ProtectItemAsync must set the snapshot resource group parameter.</summary>
    public bool RequiresSnapshotResourceGroup { get; init; }

    /// <summary>How to populate DataSourceSetInfo in protect and restore payloads.</summary>
    public DppDataSourceSetMode DataSourceSetMode { get; init; } = DppDataSourceSetMode.None;

    /// <summary>Whether additional backup parameters are needed (e.g. AKS K8s cluster settings).</summary>
    public DppBackupParametersMode BackupParametersMode { get; init; } = DppBackupParametersMode.None;

    /// <summary>Instance naming pattern for backup instances.</summary>
    public DppInstanceNamingMode InstanceNamingMode { get; init; } = DppInstanceNamingMode.Standard;


    /// <summary>The default restore approach (RP-based, PIT, or restore-as-files).</summary>
    public DppRestoreMode DefaultRestoreMode { get; init; } = DppRestoreMode.RecoveryPoint;


    /// <summary>
    /// If non-null, when the user's resource ID matches this base ARM type (e.g. "Microsoft.Storage/storageAccounts"),
    /// automatically re-map to this profile's <see cref="ArmResourceType"/>.
    /// Used for Blob (storage account → blobServices) auto-detection.
    /// </summary>
    public string? AutoDetectFromBaseResourceType { get; init; }


    /// <summary>Whether the Azure API supports updating policies for this datasource type.</summary>
    public bool SupportsPolicyUpdate { get; init; }
}
