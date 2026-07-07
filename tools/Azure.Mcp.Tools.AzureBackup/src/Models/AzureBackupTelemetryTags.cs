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

    // Unprefixed tag name shared with Microsoft.Mcp.Core's AzureTagName.SubscriptionGuid.
    // Duplicated here as a string literal because AzureTagName is internal to that assembly.
    // Emitted directly from each AzureBackup tool to ensure per-subscription telemetry is
    // captured even in namespace server mode, where the central McpRuntime tag emission
    // does not see the nested `subscription` parameter.
    public const string SubscriptionGuid = "AzSubscriptionGuid";

    /// <summary>
    /// Adds the AzSubscriptionGuid tag to the activity. Matches McpRuntime.CallToolHandler
    /// behavior by emitting any non-null subscription value (including empty string).
    /// </summary>
    public static void AddSubscriptionTag(Activity? activity, string? subscription)
    {
        if (activity is null || subscription is null)
        {
            return;
        }

        activity.SetTag(SubscriptionGuid, subscription);
    }

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
