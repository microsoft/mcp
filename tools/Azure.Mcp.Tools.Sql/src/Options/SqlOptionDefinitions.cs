// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Sql.Options;

public static class SqlOptionDefinitions
{
    public const string ServerName = "server";
    public const string DatabaseName = "database";

    public static readonly Option<string> Server = new(
        $"--{ServerName}"
    )
    {
        Description = "The Azure SQL Server name.",
        Required = true
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}"
    )
    {
        Description = "The Azure SQL Database name.",
        Required = true
    };
}
