// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Sql.Options.Database;

public sealed class DatabaseRenameOptions : BaseSqlOptions
{
    [Option(Description = "The new name for the Azure SQL Database.")]
    public required string NewDatabaseName { get; set; }

    [Option(Description = SqlOptionDescriptions.Database)]
    public required string Database { get; set; }

    [Option(Description = SqlOptionDescriptions.Server)]
    public required string Server { get; set; }
}
