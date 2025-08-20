// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Models;

public record SqlDatabase(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("sku")] DatabaseSku? Sku,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("collation")] string? Collation,
    [property: JsonPropertyName("creationDate")] DateTimeOffset? CreationDate,
    [property: JsonPropertyName("maxSizeBytes")] long? MaxSizeBytes,
    [property: JsonPropertyName("serviceLevelObjective")] string? ServiceLevelObjective,
    [property: JsonPropertyName("edition")] string? Edition,
    [property: JsonPropertyName("elasticPoolName")] string? ElasticPoolName,
    [property: JsonPropertyName("earliestRestoreDate")] DateTimeOffset? EarliestRestoreDate,
    [property: JsonPropertyName("readScale")] string? ReadScale,
    [property: JsonPropertyName("zoneRedundant")] bool? ZoneRedundant
);

public record DatabaseSku(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("capacity")] int? Capacity,
    [property: JsonPropertyName("family")] string? Family,
    [property: JsonPropertyName("size")] string? Size
);
