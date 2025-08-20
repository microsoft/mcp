// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Models;

public record SqlElasticPool(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("sku")] ElasticPoolSku? Sku,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("creationDate")] DateTimeOffset? CreationDate,
    [property: JsonPropertyName("maxSizeBytes")] long? MaxSizeBytes,
    [property: JsonPropertyName("perDatabaseSettings")] ElasticPoolPerDatabaseSettings? PerDatabaseSettings,
    [property: JsonPropertyName("zoneRedundant")] bool? ZoneRedundant,
    [property: JsonPropertyName("licenseType")] string? LicenseType,
    [property: JsonPropertyName("databaseDtuMin")] int? DatabaseDtuMin,
    [property: JsonPropertyName("databaseDtuMax")] int? DatabaseDtuMax,
    [property: JsonPropertyName("dtu")] int? Dtu,
    [property: JsonPropertyName("storageMB")] int? StorageMB
);

public record ElasticPoolSku(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("capacity")] int? Capacity,
    [property: JsonPropertyName("family")] string? Family,
    [property: JsonPropertyName("size")] string? Size
);

public record ElasticPoolPerDatabaseSettings(
    [property: JsonPropertyName("minCapacity")] double? MinCapacity,
    [property: JsonPropertyName("maxCapacity")] double? MaxCapacity
);
