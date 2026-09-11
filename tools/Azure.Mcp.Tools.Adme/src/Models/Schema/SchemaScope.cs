// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Schema;

/// <summary>
/// Defines the visibility scope of an ADME schema.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SchemaScope>))]
public enum SchemaScope
{
    SHARED,
    INTERNAL
}
