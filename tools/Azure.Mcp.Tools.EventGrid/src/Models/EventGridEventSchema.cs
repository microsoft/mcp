// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;

namespace Azure.Mcp.Tools.EventGrid.Models;

/// <summary>
/// Represents an event conforming to the EventGrid event schema for HTTP API publishing.
/// </summary>
public sealed record EventGridEventSchema(
    string Id,
    string Subject,
    string EventType,
    string DataVersion,
    JsonNode? Data,
    DateTimeOffset EventTime);
