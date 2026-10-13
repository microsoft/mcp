// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Schema;

/// <summary>
/// Defines the lifecycle status of an ADME schema.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<SchemaStatus>))]
public enum SchemaStatus
{
    PUBLISHED,
    DEVELOPMENT,
    OBSOLETE
}
