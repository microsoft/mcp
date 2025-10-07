// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Foundry.Services.Models;

/// <summary> The resource model definition representing SKU. </summary>
internal sealed class CognitiveServicesSku
{
    /// <summary> The name of the SKU. Ex - P3. It is typically a letter+number code. </summary>
    public string? Name { get; set; }
    /// <summary> If the SKU supports scale out/in then the capacity integer should be included. If scale out/in is not possible for the resource this may be omitted. </summary>
    public int? Capacity { get; set; }
}
