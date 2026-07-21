// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.MySql.Options.Database;

public sealed class DatabaseQueryOptions : MySqlDatabaseOptions
{
    [Option(Description = "Query to be executed against a MySQL database.")]
    public required string Query { get; set; }
}
