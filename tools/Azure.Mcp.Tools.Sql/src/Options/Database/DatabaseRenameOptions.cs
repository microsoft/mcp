// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Sql.Options.Database;

public class DatabaseRenameOptions : BaseDatabaseOptions
{
    [JsonPropertyName(SqlOptionDefinitions.NewDatabaseName)]
    public string? NewDatabaseName { get; set; }
}
