// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Account;

public class AccountCreateOptions : SubscriptionOptions
{
    [JsonPropertyName(StorageOptionDefinitions.AccountCreateName)]
    public string? Account { get; set; }

    [JsonPropertyName(StorageOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(StorageOptionDefinitions.SkuName)]
    public string? Sku { get; set; }

    [JsonPropertyName(StorageOptionDefinitions.AccessTierName)]
    public string? AccessTier { get; set; }

    [JsonPropertyName(StorageOptionDefinitions.EnableHierarchicalNamespaceName)]
    public bool? EnableHierarchicalNamespace { get; set; }
}
