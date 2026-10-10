// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.InfraIq.Models.Response;

public sealed class InfraIqRecommendVmSkuTopology
{
    public required int EstimatedNodeCountPerReplica { get; set; }

    public required long EstimatedTotalNodeCount { get; set; }

    public required InfraIqRecommendVmSkuAccelerator Accelerator { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
