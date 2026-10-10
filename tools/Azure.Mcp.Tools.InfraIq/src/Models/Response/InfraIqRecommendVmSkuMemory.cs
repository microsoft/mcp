// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.InfraIq.Models.Response;

public sealed class InfraIqRecommendVmSkuMemory
{
    public required double ModelWeightsGiB { get; set; }

    public required double KvCacheGiBPerRequest { get; set; }

    public required double KvCacheGiBTotal { get; set; }

    public required double ModelAndKvCacheGiB { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
