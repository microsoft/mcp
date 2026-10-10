// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.InfraIq.Models.Response;

public sealed class InfraIqRecommendVmSkuQuota
{
    public required string Status { get; set; }

    public string? Reason { get; set; }

    public InfraIqRecommendVmSkuStageError? Error { get; set; }

    public int? MaxAvailableReplicaCount { get; set; }

    public InfraIqRecommendVmSkuQuotaIncrease? Increase { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
