// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public record IoTHubQueryDiscoverResult(
    [property: JsonPropertyName("fields")] QueryDiscoveredFields Fields,
    [property: JsonPropertyName("sampleCount")] int SampleCount,
    [property: JsonPropertyName("maxCount")] int MaxCount,
    [property: JsonPropertyName("message")] string Message);
