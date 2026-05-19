// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;

public class ProtectedItemProtectOptions : BaseProtectedItemOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.PolicyName)]
    public string? Policy { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.DatasourceIdName)]
    public string? DatasourceId { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.DatasourceTypeName)]
    public string? DatasourceType { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.AksSnapshotResourceGroupName)]
    public string? AksSnapshotResourceGroup { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.AksIncludedNamespacesName)]
    public string? AksIncludedNamespaces { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.AksExcludedNamespacesName)]
    public string? AksExcludedNamespaces { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.AksLabelSelectorsName)]
    public string? AksLabelSelectors { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.AksIncludeClusterScopeResourcesName)]
    public bool AksIncludeClusterScopeResources { get; set; }
}
