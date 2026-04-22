// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;

public class ProtectedItemUndeleteOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.DatasourceIdName)]
    public string? DatasourceId { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ContainerName)]
    public string? Container { get; set; }
}
