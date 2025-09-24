// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventGrid.Models;

/// <summary>
/// CloudEvent v1.0 specification POJO for JSON deserialization.
/// Represents the standard CloudEvent format defined at https://cloudevents.io/
/// This gets converted to EventGridEvent for internal processing.
/// </summary>
public sealed class CloudEvent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("specversion")]
    public string? SpecVersion { get; set; }

    [JsonPropertyName("time")]
    public DateTimeOffset? Time { get; set; }

    [JsonPropertyName("datacontenttype")]
    public string? DataContentType { get; set; }

    [JsonPropertyName("data")]
    public JsonElement? Data { get; set; }
}

/// <summary>
/// EventGrid Event schema POJO for JSON deserialization.
/// This gets converted to EventGridEvent for internal processing.
/// Note: We still use this POJO even though EventGridEvent exists because
/// the input may have optional fields that need defaults applied.
/// </summary>
public sealed class EventGridEventInput
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("eventType")]
    public string? EventType { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    [JsonPropertyName("dataVersion")]
    public string? DataVersion { get; set; }

    [JsonPropertyName("eventTime")]
    public DateTimeOffset? EventTime { get; set; }

    [JsonPropertyName("data")]
    public JsonElement? Data { get; set; }
}

/// <summary>
/// Flexible/custom event schema POJO that supports both CloudEvents and EventGrid field names.
/// Used when the schema type is "Custom" or unknown.
/// This gets converted to EventGridEvent for internal processing.
/// </summary>
public sealed class CustomEvent
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    // CloudEvents uses "type", EventGrid uses "eventType"
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("eventType")]
    public string? EventType { get; set; }

    // CloudEvents uses "source", EventGrid uses "subject"
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    // CloudEvents uses "specversion", EventGrid uses "dataVersion"
    [JsonPropertyName("specversion")]
    public string? SpecVersion { get; set; }

    [JsonPropertyName("dataVersion")]
    public string? DataVersion { get; set; }

    // CloudEvents uses "time", EventGrid uses "eventTime"
    [JsonPropertyName("time")]
    public DateTimeOffset? Time { get; set; }

    [JsonPropertyName("eventTime")]
    public DateTimeOffset? EventTime { get; set; }

    [JsonPropertyName("datacontenttype")]
    public string? DataContentType { get; set; }

    [JsonPropertyName("data")]
    public JsonElement? Data { get; set; }
}
