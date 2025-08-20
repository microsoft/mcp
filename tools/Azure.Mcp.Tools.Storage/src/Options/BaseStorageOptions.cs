// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Storage.Options;

public class BaseStorageOptions : SubscriptionOptions
{
    [JsonPropertyName(StorageOptionDefinitions.AccountName)]
    public string? Account { get; set; }
}
