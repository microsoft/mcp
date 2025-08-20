// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Postgres.Options.Table;

public class TableSchemaGetOptions : BasePostgresOptions
{
    [JsonPropertyName(PostgresOptionDefinitions.TableName)]
    public string? Table { get; set; }
}
