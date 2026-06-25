// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.Database;

public sealed class DatabaseCreateOptions : BaseSqlOptions
{
    [Option(Description = SqlOptionDescriptions.SkuName)]
    public string? SkuName { get; set; }

    [Option(Description = SqlOptionDescriptions.SkuTier)]
    public string? SkuTier { get; set; }

    [Option(Description = SqlOptionDescriptions.SkuCapacity)]
    public int? SkuCapacity { get; set; }

    [Option(Description = SqlOptionDescriptions.Collation)]
    public string? Collation { get; set; }

    [Option(Description = SqlOptionDescriptions.MaxSizeBytes)]
    public long? MaxSizeBytes { get; set; }

    [Option(Description = SqlOptionDescriptions.ElasticPoolName)]
    public string? ElasticPoolName { get; set; }

    [Option(Description = SqlOptionDescriptions.ZoneRedundant)]
    public bool? ZoneRedundant { get; set; }

    [Option(Description = SqlOptionDescriptions.ReadScale)]
    public string? ReadScale { get; set; }

    [Option(Description = SqlOptionDescriptions.Database)]
    public required string Database { get; set; }

    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
