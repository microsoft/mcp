// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.MySql.Options.Table;

public class TableSchemaGetOptions : MySqlDatabaseOptions
{
    [JsonPropertyName(MySqlOptionDefinitions.TableName)]
    public string? Table { get; set; }
}
