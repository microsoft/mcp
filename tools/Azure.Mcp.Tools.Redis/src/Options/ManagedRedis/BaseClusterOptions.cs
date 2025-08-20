// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Redis.Options.ManagedRedis;

public class BaseClusterOptions : SubscriptionOptions
{
    [JsonPropertyName(RedisOptionDefinitions.ClusterName)]
    public string? Cluster { get; set; }
}
