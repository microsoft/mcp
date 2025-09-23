// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Options.Database;

public class DatabaseUpdateOptions : BaseDatabaseOptions
{
    [JsonPropertyName(SqlOptionDefinitions.SkuName)]
    public string? SkuName { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.SkuTier)]
    public string? SkuTier { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.SkuCapacity)]
    public int? SkuCapacity { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.Collation)]
    public string? Collation { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.MaxSizeBytes)]
    public long? MaxSizeBytes { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.ElasticPoolName)]
    public string? ElasticPoolName { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.ZoneRedundant)]
    public bool? ZoneRedundant { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.ReadScale)]
    public string? ReadScale { get; set; }
}
