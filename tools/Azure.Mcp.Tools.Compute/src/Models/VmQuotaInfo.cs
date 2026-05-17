// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// Reports compute (vCPU) quota usage for a single SKU family or aggregate in a region.
/// </summary>
public sealed record VmQuotaInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("localizedName")] string? LocalizedName,
    [property: JsonPropertyName("unit")] string? Unit,
    [property: JsonPropertyName("currentValue")] long CurrentValue,
    [property: JsonPropertyName("limit")] long Limit,
    [property: JsonPropertyName("available")] long Available,
    [property: JsonPropertyName("percentUsed")] double PercentUsed,
    [property: JsonPropertyName("nearLimit")] bool NearLimit,
    [property: JsonPropertyName("status")] string Status);
