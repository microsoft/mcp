// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.InfraIq.Models.Response;

public sealed class InfraIqRecommendVmSkuQuotaLimit
{
    public required string Name { get; set; }

    public required long CurrentUsage { get; set; }

    public required long CurrentLimit { get; set; }

    public required long RequiredLimit { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
