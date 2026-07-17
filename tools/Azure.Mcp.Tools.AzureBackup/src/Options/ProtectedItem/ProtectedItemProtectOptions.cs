// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;

public sealed class ProtectedItemProtectOptions : BaseProtectedItemOptions
{
    [Option(Description = AzureBackupOptionDefinitions.Policy)]
    public required string Policy { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.DatasourceId)]
    public required string DatasourceId { get; set; }

    [Option(Description = "The workload type hint: VM, SQL, SAPHANA, SAPASE, AzureFileShare (RSV types); AzureDisk, AzureBlob, AKS, ElasticSAN, PostgreSQLFlexible, ADLS, CosmosDB (DPP types). Also accepts aliases like AzureVM, SQLDatabase, etc.")]
    public string? DatasourceType { get; set; }

    [Option(Description = "Resource group used to store AKS volume snapshots created by Backup. DPP AKS only.")]
    public string? AksSnapshotResourceGroup { get; set; }

    [Option(Description = "Comma-separated list of namespaces to include in the AKS backup policy default scope. DPP AKS only.")]
    public string? AksIncludedNamespaces { get; set; }

    [Option(Description = "Comma-separated list of namespaces to exclude from the AKS backup policy default scope. DPP AKS only.")]
    public string? AksExcludedNamespaces { get; set; }

    [Option(Description = "Comma-separated label selectors (e.g. 'app=frontend,tier=web') applied to the AKS backup policy default scope. DPP AKS only.")]
    public string? AksLabelSelectors { get; set; }

    [Option(Description = "Include cluster-scoped resources in the AKS backup policy. DPP AKS only.")]
    public bool AksIncludeClusterScopeResources { get; set; }
}
