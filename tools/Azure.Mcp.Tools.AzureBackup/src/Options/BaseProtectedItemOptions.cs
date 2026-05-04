// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options;

public class BaseProtectedItemOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.ProtectedItemName)]
    public string? ProtectedItem { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ContainerName)]
    public string? Container { get; set; }
}
