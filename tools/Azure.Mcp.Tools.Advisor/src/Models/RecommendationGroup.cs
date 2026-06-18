// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;


public record RecommendationGroup(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("count")] int Count);
