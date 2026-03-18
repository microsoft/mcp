// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Governance;

public class GovernanceFindUnprotectedOptions : SubscriptionOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.ResourceTypeFilterName)]
    public string? ResourceTypeFilter { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ResourceGroupFilterName)]
    public string? ResourceGroupFilter { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.TagFilterName)]
    public string? TagFilter { get; set; }
}
