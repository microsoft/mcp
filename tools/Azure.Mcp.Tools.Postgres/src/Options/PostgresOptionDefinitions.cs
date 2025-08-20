// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Postgres.Options;

public static class PostgresOptionDefinitions
{
    public const string UserName = "user";
    public const string ServerName = "server";
    public const string DatabaseName = "database";
    public const string TableName = "table";
    public const string QueryText = "query";
    public const string ParamName = "param";
    public const string ValueName = "value";

    public static readonly Option<string> User = new(
        $"--{UserName}",
        "The user name to access PostgreSQL server."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Server = new(
        $"--{ServerName}",
        "The PostgreSQL server to be accessed."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}",
        "The PostgreSQL database to be access."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Table = new(
        $"--{TableName}",
        "The PostgreSQL table to be access."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryText}",
        "Query to be executed against a PostgreSQL database."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Param = new(
        $"--{ParamName}",
        "The PostgreSQL parameter to be accessed."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Value = new(
        $"--{ValueName}",
        "The value to set for the PostgreSQL parameter."
    )
    {
        IsRequired = true
    };
}
