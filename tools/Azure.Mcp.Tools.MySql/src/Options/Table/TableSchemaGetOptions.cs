// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.MySql.Options.Table;

public sealed class TableSchemaGetOptions : MySqlDatabaseOptions
{
    [Option(Description = "The MySQL table to be accessed.")]
    public required string Table { get; set; }
}
