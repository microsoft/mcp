// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options;

public class BaseComputeOptions : SubscriptionOptions
{
    [JsonPropertyName(ComputeOptionDefinitions.ResourceGroupName)]
    public new string? ResourceGroup { get; set; }
}
