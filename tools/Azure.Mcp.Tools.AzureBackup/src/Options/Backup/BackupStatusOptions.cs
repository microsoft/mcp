// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Backup;

public class BackupStatusOptions : SubscriptionOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.DatasourceIdName)]
    public string? DatasourceId { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.LocationName)]
    public string? Location { get; set; }
}
