// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Kusto.Services.Models;

/// <summary>
/// A class representing the Kusto SKU data model.
/// </summary>
internal sealed class KustoSku
{
    /// <summary> SKU name. </summary>
    public string? Name { get; set; }
    /// <summary> The number of instances of the cluster. </summary>
    public int? Capacity { get; set; }
    /// <summary> SKU tier. </summary>
    public string? Tier { get; set; }
}
