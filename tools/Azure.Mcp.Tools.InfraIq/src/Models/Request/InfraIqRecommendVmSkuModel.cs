// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Request;

public sealed class InfraIqRecommendVmSkuModel
{
    public string? HuggingFaceModelId { get; set; }

    public long? ParameterCount { get; set; }

    public string? WeightPrecision { get; set; }

    public int? NumLayers { get; set; }

    public int? NumKeyValueHeads { get; set; }

    public int? HeadDim { get; set; }
}
