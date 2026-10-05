// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.InfraIq.Models.Response;

public sealed class InfraIqRecommendVmSkuSizing
{
    public required InfraIqRecommendVmSkuSizingModel Model { get; set; }

    public required InfraIqRecommendVmSkuSizingWorkload Workload { get; set; }

    public required InfraIqRecommendVmSkuMemory Memory { get; set; }

    public required InfraIqRecommendVmSkuSizingBasis Basis { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
