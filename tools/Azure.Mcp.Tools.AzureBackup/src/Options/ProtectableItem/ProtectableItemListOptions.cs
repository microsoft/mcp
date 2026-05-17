// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectableItem;

public class ProtectableItemListOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.WorkloadTypeName)]
    public string? WorkloadType { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ContainerName)]
    public string? Container { get; set; }
}
