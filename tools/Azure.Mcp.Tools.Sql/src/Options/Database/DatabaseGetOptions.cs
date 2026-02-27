// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Options.Database;

public class DatabaseGetOptions : BaseSqlOptions
{
    [JsonPropertyName(SqlOptionDefinitions.DatabaseName)]
    public string? Database { get; set; }
}
