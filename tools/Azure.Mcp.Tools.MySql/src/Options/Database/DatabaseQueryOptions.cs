// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.MySql.Options.Database;

public class DatabaseQueryOptions : MySqlDatabaseOptions
{
    [JsonPropertyName(MySqlOptionDefinitions.QueryText)]
    public string? Query { get; set; }
}
