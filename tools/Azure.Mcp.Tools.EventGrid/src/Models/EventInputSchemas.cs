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
public sealed record CloudEvent(
    string? Id,
    string? Type,
    string? Source,
    string? Subject,
    string? SpecVersion,
    DateTimeOffset? Time,
    [property: JsonPropertyName("datacontenttype")] string? DataContentType,
    JsonElement? Data);

/// <summary>
/// EventGrid Event schema POJO for JSON deserialization.
/// This gets converted to EventGridEvent for internal processing.
/// Note: We still use this POJO even though EventGridEvent exists because
/// the input may have optional fields that need defaults applied.
/// </summary>
public sealed record EventGridEventInput(
    string? Id,
    string? EventType,
    string? Subject,
    string? DataVersion,
    DateTimeOffset? EventTime,
    JsonElement? Data);

/// <summary>
/// Flexible/custom event schema POJO that supports both CloudEvents and EventGrid field names.
/// Used when the schema type is "Custom" or unknown.
/// This gets converted to EventGridEvent for internal processing.
/// </summary>
public sealed record CustomEvent(
    string? Id,
    // CloudEvents uses "type", EventGrid uses "eventType"
    string? Type,
    string? EventType,
    // CloudEvents uses "source", EventGrid uses "subject"
    string? Source,
    string? Subject,
    // CloudEvents uses "specversion", EventGrid uses "dataVersion"
    string? SpecVersion,
    string? DataVersion,
    // CloudEvents uses "time", EventGrid uses "eventTime"
    DateTimeOffset? Time,
    DateTimeOffset? EventTime,
    [property: JsonPropertyName("datacontenttype")] string? DataContentType,
    JsonElement? Data);
