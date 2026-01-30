// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options;

public class BaseComputeOptions : SubscriptionOptions
{
    [JsonPropertyName(OptionDefinitions.Common.ResourceGroupName)]
    public new string? ResourceGroup { get; set; }
}
