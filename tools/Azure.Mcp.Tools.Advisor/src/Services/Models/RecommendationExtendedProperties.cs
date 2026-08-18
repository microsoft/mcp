// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary> Dynamic type-specific properties returned with an Advisor recommendation. </summary>
internal sealed class RecommendationExtendedProperties
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Properties { get; set; }
}
