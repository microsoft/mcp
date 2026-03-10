// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vm;

/// <summary>
/// Options for the VM SKU list command.
/// </summary>
public class VmSkuListOptions : SubscriptionOptions
{
    /// <summary>
    /// The Azure location/region to list SKUs for.
    /// </summary>
    [JsonPropertyName(ComputeOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    /// <summary>
    /// Optional filter by SKU name (exact match).
    /// </summary>
    [JsonPropertyName(ComputeOptionDefinitions.SkuName)]
    public string? Sku { get; set; }
}
