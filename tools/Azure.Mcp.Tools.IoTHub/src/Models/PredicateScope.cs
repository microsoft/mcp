// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.IoTHub.Models;

/// <summary>
/// Identifies the property scope a <see cref="QueryPredicate"/> targets within an IoT Hub device document.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<PredicateScope>))]
public enum PredicateScope
{
    /// <summary>A top-level device property (e.g. deviceId, status, connectionState). No prefix is applied.</summary>
    Device,

    /// <summary>A device tag. Compiles to a <c>tags.</c> prefixed field path.</summary>
    Tags,

    /// <summary>A desired twin property. Compiles to a <c>properties.desired.</c> prefixed field path.</summary>
    Desired,

    /// <summary>A reported twin property. Compiles to a <c>properties.reported.</c> prefixed field path.</summary>
    Reported
}
