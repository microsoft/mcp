// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options.Account;

public class AccountCreateOptions : SubscriptionOptions
{
    [JsonPropertyName("account-name")]
    public string? AccountName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("sku")]
    public string? Sku { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("access-tier")]
    public string? AccessTier { get; set; }

    [JsonPropertyName("enable-https-traffic-only")]
    public bool? EnableHttpsTrafficOnly { get; set; }

    [JsonPropertyName("allow-blob-public-access")]
    public bool? AllowBlobPublicAccess { get; set; }

    [JsonPropertyName("enable-hierarchical-namespace")]
    public bool? EnableHierarchicalNamespace { get; set; }
}
