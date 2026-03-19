// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Identifies the SDK protected-item type to construct for RSV protect/stop/resume/modify operations.
/// </summary>
public enum RsvProtectedItemType
{
    /// <summary>IaasComputeVmProtectedItem — Azure Virtual Machines.</summary>
    IaasVm,

    /// <summary>VmWorkloadSqlDatabaseProtectedItem — SQL databases in Azure VMs.</summary>
    SqlDatabase,

    /// <summary>VmWorkloadSapHanaDatabaseProtectedItem — SAP HANA databases in Azure VMs.</summary>
    SapHanaDatabase,

    /// <summary>FileshareProtectedItem — Azure File Shares.</summary>
    AzureFileShare,
}

/// <summary>
/// Identifies the SDK policy type to construct for RSV policy-create operations.
/// </summary>
public enum RsvPolicyType
{
    /// <summary>IaasVmProtectionPolicy — for VM backup.</summary>
    IaasVm,

    /// <summary>VmWorkloadProtectionPolicy — for SQL/HANA/ASE workload backup (Full + Log sub-policies).</summary>
    VmWorkload,

    /// <summary>AzureFileShareProtectionPolicy — for Azure File Share backup.</summary>
    AzureFileShare,
}

/// <summary>
/// Identifies the SDK backup content type to construct for RSV trigger-backup operations.
/// </summary>
public enum RsvBackupContentType
{
    /// <summary>IaasVmBackupContent — for VM on-demand backup.</summary>
    IaasVm,

    /// <summary>WorkloadBackupContent — for SQL/HANA/ASE on-demand workload backup.</summary>
    Workload,

    /// <summary>AzureFileShareBackupContent — for File Share on-demand backup.</summary>
    AzureFileShare,
}

/// <summary>
/// Identifies the SDK restore content type to construct for RSV trigger-restore operations.
/// </summary>
public enum RsvRestoreContentType
{
    /// <summary>IaasVmRestoreContent — OLR/ALR/RestoreDisks for VMs.</summary>
    IaasVm,

    /// <summary>WorkloadSqlRestoreContent — SQL database OLR/ALR with data directory mappings.</summary>
    SqlRestore,

    /// <summary>WorkloadSapHanaRestoreContent — SAP HANA database OLR/ALR.</summary>
    SapHanaRestore,

    /// <summary>AzureFileShareRestoreContent — File Share restore.</summary>
    AzureFileShareRestore,
}

/// <summary>
/// Immutable profile describing all configuration aspects of an RSV datasource type.
/// Eliminates hardcoded if/else branching by centralizing datasource-specific SDK type selection.
/// </summary>
/// <remarks>
/// AOT-safe: no reflection, no Func delegates — pure data record with enum-driven dispatch.
/// RSV SDK uses polymorphic types (IaasVmProtectedItem, VmWorkloadSqlDatabaseProtectedItem, etc.)
/// so the profile maps user-facing names to the correct enum values that drive construction.
/// </remarks>
public sealed record RsvDatasourceProfile
{

    /// <summary>Friendly name used for user-facing resolution (e.g. "VM", "SQL").</summary>
    public required string FriendlyName { get; init; }

    /// <summary>Alternative user-supplied names that resolve to this profile.</summary>
    public string[] Aliases { get; init; } = [];


    /// <summary>
    /// Whether this is a "workload" type (SQL, HANA, ASE) that runs inside a VM.
    /// Workload types require container registration, workload discovery, and use VMAppContainer naming.
    /// </summary>
    public bool IsWorkloadType { get; init; }


    /// <summary>Which SDK ProtectedItem type to construct for protect/stop/resume/modify.</summary>
    public RsvProtectedItemType ProtectedItemType { get; init; } = RsvProtectedItemType.IaasVm;

    /// <summary>Which SDK Policy type to construct for policy-create.</summary>
    public RsvPolicyType PolicyType { get; init; } = RsvPolicyType.IaasVm;

    /// <summary>Which SDK BackupContent type to construct for trigger-backup.</summary>
    public RsvBackupContentType BackupContentType { get; init; } = RsvBackupContentType.IaasVm;

    /// <summary>Which SDK RestoreContent type to construct for trigger-restore.</summary>
    public RsvRestoreContentType RestoreContentType { get; init; } = RsvRestoreContentType.IaasVm;


    /// <summary>
    /// The Azure API workload type string (e.g. "SQLDataBase", "SAPHanaDatabase").
    /// Used when creating workload policies and registering containers.
    /// Null for non-workload types (VM, FileShare).
    /// </summary>
    public string? ApiWorkloadType { get; init; }


    /// <summary>
    /// Prefix for the container name derivation.
    /// VM: "IaasVMContainer;iaasvmcontainerv2",
    /// SQL/HANA/ASE: "VMAppContainer;Compute",
    /// FileShare: "StorageContainer".
    /// </summary>
    public required string ContainerNamePrefix { get; init; }


    /// <summary>Whether container registration is required before protection (workload types).</summary>
    public bool RequiresContainerRegistration { get; init; }

    /// <summary>Whether container inquiry (workload discovery) is required before protection.</summary>
    public bool RequiresContainerInquiry { get; init; }

    /// <summary>Whether VM discovery/refresh is required before protection (VM type).</summary>
    public bool RequiresContainerDiscovery { get; init; }

    /// <summary>Whether auto-protection is supported (SQL workloads).</summary>
    public bool SupportsAutoProtect { get; init; }

    /// <summary>Whether the Azure API supports policy updates for this datasource type.</summary>
    public bool SupportsPolicyUpdate { get; init; } = true;
}
