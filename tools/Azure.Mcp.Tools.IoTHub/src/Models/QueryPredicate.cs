// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

/// <summary>
/// A single, structured filter that is compiled into one clause of an IoT Hub query <c>WHERE</c> expression.
/// </summary>
public class QueryPredicate
{
    /// <summary>The property scope (device, tags, desired, or reported) the predicate targets.</summary>
    [JsonPropertyName("scope")]
    public PredicateScope Scope { get; set; }

    /// <summary>The field path within the scope (e.g. <c>floor</c>, <c>temperature</c>, <c>deviceId</c>).</summary>
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    /// <summary>The comparison operator to apply between the field and the value.</summary>
    [JsonPropertyName("operator")]
    public PredicateOperator Operator { get; set; }

    /// <summary>The literal value to compare against. Strings are emitted quoted; numbers and booleans are emitted as-is.</summary>
    [JsonPropertyName("value")]
    public JsonElement Value { get; set; }
}
