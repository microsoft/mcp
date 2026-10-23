// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

public class QueryDiscoveredFields
{
    [JsonPropertyName("device")]
    public List<QueryDiscoveredField> Device { get; set; } = [];

    [JsonPropertyName("tags")]
    public List<QueryDiscoveredField> Tags { get; set; } = [];

    [JsonPropertyName("desired")]
    public List<QueryDiscoveredField> Desired { get; set; } = [];

    [JsonPropertyName("reported")]
    public List<QueryDiscoveredField> Reported { get; set; } = [];
}
