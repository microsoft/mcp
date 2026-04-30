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
}
