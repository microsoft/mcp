// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Sql.Options;

public static class SqlOptionDefinitions
{
    public const string ServerName = "server";
    public const string DatabaseName = "database";

    public static readonly Option<string> Server = new(
        $"--{ServerName}",
        "The Azure SQL Server name."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}",
        "The Azure SQL Database name."
    )
    {
        IsRequired = true
    };
}
