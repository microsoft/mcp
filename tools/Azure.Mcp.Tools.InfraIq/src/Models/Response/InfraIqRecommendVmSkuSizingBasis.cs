// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.InfraIq.Models.Response;

public sealed class InfraIqRecommendVmSkuSizingBasis
{
    public required string ParameterCountSource { get; set; }

    public required string ModelWeightsCalculation { get; set; }

    public required string ContextLengthSource { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
