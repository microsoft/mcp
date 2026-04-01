// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Options;

public class BaseCosmosOptions : SubscriptionOptions
{
    [JsonPropertyName(CosmosOptionDefinitions.AccountName)]
    public string? Account { get; set; }
}
