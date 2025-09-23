// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventGrid.Models;

/// <summary>
/// Represents an event conforming to the EventGrid event schema for HTTP API publishing.
/// </summary>
public sealed class EventGridEventSchema
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("subject")]
    public required string Subject { get; set; }

    [JsonPropertyName("eventType")]
    public required string EventType { get; set; }

    [JsonPropertyName("dataVersion")]
    public required string DataVersion { get; set; }

    [JsonPropertyName("data")]
    public JsonNode? Data { get; set; }

    [JsonPropertyName("eventTime")]
    public DateTimeOffset EventTime { get; set; }
}
