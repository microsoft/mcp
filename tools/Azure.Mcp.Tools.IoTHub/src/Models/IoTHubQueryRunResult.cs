// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record IoTHubQueryRunResult(
    [property: JsonPropertyName("items")] List<JsonElement> Items,
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("hasMore")] bool HasMore,
    [property: JsonPropertyName("continuationToken")] string? ContinuationToken,
    [property: JsonPropertyName("message")] string? Message);
