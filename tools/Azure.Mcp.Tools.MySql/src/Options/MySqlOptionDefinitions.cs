// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.MySql.Options;

public static class MySqlOptionDefinitions
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
        "The user name to access MySQL server."
    )
    {
        Required = true
    };

    public static readonly Option<string> Server = new(
        $"--{ServerName}",
        "The MySQL server to be accessed."
    )
    {
        Required = true
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}",
        "The MySQL database to be accessed."
    )
    {
        Required = true
    };

    public static readonly Option<string> Table = new(
        $"--{TableName}",
        "The MySQL table to be accessed."
    )
    {
        Required = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryText}",
        "Query to be executed against a MySQL database."
    )
    {
        Required = true
    };

    public static readonly Option<string> Param = new(
        $"--{ParamName}",
        "The MySQL parameter to be accessed."
    )
    {
        Required = true
    };

    public static readonly Option<string> Value = new(
        $"--{ValueName}",
        "The value to set for the MySQL parameter."
    )
    {
        Required = true
    };
}
