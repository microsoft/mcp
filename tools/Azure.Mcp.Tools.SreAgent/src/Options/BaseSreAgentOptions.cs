// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options;

public class BaseSreAgentOptions : SubscriptionOptions
{
    [JsonPropertyName(SreAgentOptionDefinitions.AgentNameName)]
    public string? Agent { get; set; }
}
