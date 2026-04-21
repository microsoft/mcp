// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options;

public class BaseComputeOptions : SubscriptionOptions
{
    [JsonPropertyName(OptionDefinitions.Common.ResourceGroupName)]
    public new string? ResourceGroup { get; set; }
}
