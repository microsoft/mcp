// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace Azure.Mcp.Tools.AzureBackup.Models;

public static class AzureBackupTelemetryTags
{
    private static string AddPrefix(string tagName) => $"azurebackup/{tagName}";

    public static readonly string VaultType = AddPrefix("VaultType");
    public static readonly string WorkloadType = AddPrefix("WorkloadType");
    public static readonly string DatasourceType = AddPrefix("DatasourceType");
    public static readonly string OperationScope = AddPrefix("OperationScope");

    /// <summary>
    /// Normalizes the vault type to canonical lowercase values (rsv/dpp).
    /// Returns "auto" when the input is null or empty (user didn't specify --vault-type).
    /// </summary>
    public static string NormalizeVaultType(string? vaultType) =>
        string.IsNullOrWhiteSpace(vaultType) ? "auto" : vaultType.ToLowerInvariant();

    /// <summary>
    /// Normalizes the workload type to canonical lowercase for consistent telemetry.
    /// Returns "unspecified" when the input is null or empty.
    /// </summary>
    public static string NormalizeWorkloadType(string? workloadType) =>
        string.IsNullOrWhiteSpace(workloadType) ? "unspecified" : workloadType.ToLowerInvariant();

    /// <summary>
    /// Adds a normalized vault type tag to the activity.
    /// </summary>
    public static void AddVaultTags(Activity? activity, string? vaultType)
    {
        activity?.AddTag(VaultType, NormalizeVaultType(vaultType));
    }

    /// <summary>
    /// Adds normalized vault type and workload type tags to the activity.
    /// </summary>
    public static void AddVaultAndWorkloadTags(Activity? activity, string? vaultType, string? workloadType)
    {
        activity?.AddTag(VaultType, NormalizeVaultType(vaultType));
        activity?.AddTag(WorkloadType, NormalizeWorkloadType(workloadType));
    }
}
