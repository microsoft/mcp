// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Azure.Models;


/// <summary>
/// A class representing the Resource Sku data model.
/// </summary>
public sealed class ResourceSku
{
    /// <summary> Name of this SKU. </summary>
    public string? Name { get; set; }
    /// <summary> The billing tier of this particular SKU. </summary>
    public string? Tier { get; set; }
    /// <summary> The throughput units. </summary>
    public int? Capacity { get; set; }
}
