// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// Central registry of all RSV (Recovery Services Vault) datasource profiles.
/// Acts as a single source of truth for datasource-specific SDK type selection,
/// replacing scattered if/else and switch checks in RsvBackupOperations.
///
/// To add a new RSV datasource type:
///   1. Add a static RsvDatasourceProfile instance below
///   2. Register it in the AllProfiles array
///   3. If it uses new SDK types, add enum values to RsvDatasourceProfile.cs
///    -  Minimises changes needed in RsvBackupOperations.
/// </summary>
public static class RsvDatasourceRegistry
{

    public static readonly RsvDatasourceProfile IaasVm = new()
    {
        FriendlyName = "VM",
        Aliases = ["vm", "iaasvm", "azurevm", "azureiaasvm", "virtualmachine", "iaasvmcontainer"],
        IsWorkloadType = false,
        ProtectedItemType = RsvProtectedItemType.IaasVm,
        PolicyType = RsvPolicyType.IaasVm,
        BackupContentType = RsvBackupContentType.IaasVm,
        RestoreContentType = RsvRestoreContentType.IaasVm,
        ContainerNamePrefix = "IaasVMContainer;iaasvmcontainerv2",
        RequiresContainerDiscovery = true,
        SupportsPolicyUpdate = true,
    };

    /// <summary>
    /// SQL in Azure VM: VmWorkload policy with Full + Log sub-policies.
    /// Stage 3: Add Differential (optional) sub-policy. Full and Differential cannot be on same day.
    /// </summary>
    public static readonly RsvDatasourceProfile SqlDatabase = new()
    {
        FriendlyName = "SQL",
        Aliases = ["sql", "sqldatabase", "sqldb", "mssql", "azuresql"],
        IsWorkloadType = true,
        ProtectedItemType = RsvProtectedItemType.SqlDatabase,
        PolicyType = RsvPolicyType.VmWorkload,
        BackupContentType = RsvBackupContentType.Workload,
        RestoreContentType = RsvRestoreContentType.SqlRestore,
        ApiWorkloadType = "SQLDataBase",
        ContainerNamePrefix = "VMAppContainer;Compute",
        RequiresContainerRegistration = true,
        RequiresContainerInquiry = true,
        SupportsAutoProtect = true,
        SupportsPolicyUpdate = true,
    };

    /// <summary>
    /// SAP HANA in Azure VM: VmWorkload policy with Full + Log (mandatory) sub-policies.
    /// Stage 3: Add Differential (optional) and Incremental (optional) sub-policies.
    /// Full, Differential, and Incremental cannot be scheduled on the same day.
    /// </summary>
    public static readonly RsvDatasourceProfile SapHanaDatabase = new()
    {
        FriendlyName = "SAPHANA",
        Aliases = ["saphana", "saphanadatabase", "saphanadb", "hana"],
        IsWorkloadType = true,
        ProtectedItemType = RsvProtectedItemType.SapHanaDatabase,
        PolicyType = RsvPolicyType.VmWorkload,
        BackupContentType = RsvBackupContentType.Workload,
        RestoreContentType = RsvRestoreContentType.SapHanaRestore,
        ApiWorkloadType = "SAPHanaDatabase",
        ContainerNamePrefix = "VMAppContainer;Compute",
        RequiresContainerRegistration = true,
        RequiresContainerInquiry = true,
        SupportsPolicyUpdate = true,
    };

    /// <summary>
    /// SAP ASE in Azure VM: Same sub-policy support as SAP HANA.
    /// Full (mandatory) + Log (mandatory) + Differential (optional) + Incremental (optional).
    /// Full, Differential, and Incremental cannot be scheduled on the same day.
    /// </summary>
    public static readonly RsvDatasourceProfile SapAse = new()
    {
        FriendlyName = "SAPASE",
        Aliases = ["sapase", "ase", "sybase"],
        IsWorkloadType = true,
        ProtectedItemType = RsvProtectedItemType.SqlDatabase, // ASE uses same SDK type pattern as SQL
        PolicyType = RsvPolicyType.VmWorkload,
        BackupContentType = RsvBackupContentType.Workload,
        RestoreContentType = RsvRestoreContentType.SqlRestore,
        ApiWorkloadType = "SAPAseDatabase",
        ContainerNamePrefix = "VMAppContainer;Compute",
        RequiresContainerRegistration = true,
        RequiresContainerInquiry = true,
        SupportsPolicyUpdate = true,
    };

    public static readonly RsvDatasourceProfile AzureFileShare = new()
    {
        FriendlyName = "AzureFileShare",
        Aliases = ["azurefileshare", "fileshare", "afs"],
        IsWorkloadType = false,
        ProtectedItemType = RsvProtectedItemType.AzureFileShare,
        PolicyType = RsvPolicyType.AzureFileShare,
        BackupContentType = RsvBackupContentType.AzureFileShare,
        RestoreContentType = RsvRestoreContentType.AzureFileShareRestore,
        ContainerNamePrefix = "StorageContainer;Storage",
        SupportsPolicyUpdate = true,
    };


    /// <summary>All registered RSV datasource profiles.</summary>
    public static readonly RsvDatasourceProfile[] AllProfiles =
    [
        IaasVm,
        SqlDatabase,
        SapHanaDatabase,
        SapAse,
        AzureFileShare,
    ];

    /// <summary>
    /// Returns the list of all known workload type names (friendly names plus aliases).
    /// Useful for user-facing validation messages.
    /// </summary>
    public static IReadOnlyList<string> KnownTypeNames { get; } = AllProfiles
        .SelectMany(p => new[] { p.FriendlyName }.Concat(p.Aliases))
        .ToArray();

    /// <summary>
    /// Resolves a user-supplied datasource type to the matching RSV profile.
    /// Case-insensitive match against FriendlyName and Aliases.
    /// Returns null if no match is found (caller should default to VM).
    /// </summary>
    public static RsvDatasourceProfile? Resolve(string? datasourceType)
    {
        if (string.IsNullOrEmpty(datasourceType))
        {
            return null;
        }

        var normalised = datasourceType.ToLowerInvariant();

        foreach (var profile in AllProfiles)
        {
            if (normalised.Equals(profile.FriendlyName, StringComparison.OrdinalIgnoreCase))
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

        return null;
    }

    /// <summary>
    /// Resolves a datasource type to a profile, defaulting to VM if no match.
    /// When datasourceType is explicitly provided but not recognized, throws ArgumentException.
    /// </summary>
    public static RsvDatasourceProfile ResolveOrDefault(string? datasourceType)
    {
        if (string.IsNullOrEmpty(datasourceType))
        {
            return IaasVm;
        }

        return Resolve(datasourceType)
            ?? throw new ArgumentException(
                $"Unknown RSV workload type '{datasourceType}'. " +
                $"Supported types: {string.Join(", ", KnownTypeNames)}.",
                nameof(datasourceType));
    }

    /// <summary>
    /// Determines the profile from an existing protected item's naming conventions.
    /// RSV protected items encode their type in the name prefix.
    /// </summary>
    public static RsvDatasourceProfile ResolveFromProtectedItemName(string protectedItemName, string? containerName)
    {
        var itemLower = protectedItemName.ToLowerInvariant();
        var containerLower = containerName?.ToLowerInvariant() ?? string.Empty;

        if (itemLower.StartsWith("sqldatabase;", StringComparison.OrdinalIgnoreCase) ||
            itemLower.StartsWith("sqlinstance;", StringComparison.OrdinalIgnoreCase))
        {
            return SqlDatabase;
        }

        if (itemLower.StartsWith("saphanadatabase;", StringComparison.OrdinalIgnoreCase) ||
            itemLower.StartsWith("saphanainstance;", StringComparison.OrdinalIgnoreCase))
        {
            return SapHanaDatabase;
        }

        if (itemLower.StartsWith("sapasedatabase;", StringComparison.OrdinalIgnoreCase))
        {
            return SapAse;
        }

        if (itemLower.StartsWith("azurefileshare;", StringComparison.OrdinalIgnoreCase) ||
            containerLower.StartsWith("storagecontainer;", StringComparison.OrdinalIgnoreCase))
        {
            return AzureFileShare;
        }

        if (containerLower.StartsWith("vmappcontainer", StringComparison.OrdinalIgnoreCase))
        {
            return SqlDatabase;
        }

        return IaasVm;
    }
}
