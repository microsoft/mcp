// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record QueryDiscoveredField(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("examples")] List<JsonElement> Examples);
