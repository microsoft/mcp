// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Option;

namespace Azure.Mcp.Core.Options;

public class SubscriptionOptions : GlobalOptions
{
    [JsonPropertyName(OptionDefinitions.Common.SubscriptionName)]
    public string? Subscription { get; set; }
}
