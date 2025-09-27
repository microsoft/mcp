// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Options.Database;

public class DatabaseExportOptions : BaseDatabaseOptions
{
    [JsonPropertyName(SqlOptionDefinitions.StorageUri)]
    public string? StorageUri { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.StorageKey)]
    public string? StorageKey { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.StorageKeyType)]
    public string? StorageKeyType { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.AdminUser)]
    public string? AdminUser { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.AdminPassword)]
    public string? AdminPassword { get; set; }

    [JsonPropertyName(SqlOptionDefinitions.AuthType)]
    public string? AuthType { get; set; }
}