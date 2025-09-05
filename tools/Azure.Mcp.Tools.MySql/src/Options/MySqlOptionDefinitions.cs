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
        $"--{UserName}"
    )
    {
        Description = "The user name to access MySQL server.",
        Required = true
    };

    public static readonly Option<string> Server = new(
        $"--{ServerName}"
    )
    {
        Description = "The MySQL server to be accessed.",
        Required = true
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}"
    )
    {
        Description = "The MySQL database to be accessed.",
        Required = true
    };

    public static readonly Option<string> Table = new(
        $"--{TableName}"
    )
    {
        Description = "The MySQL table to be accessed.",
        Required = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryText}"
    )
    {
        Description = "Query to be executed against a MySQL database.",
        Required = true
    };

    public static readonly Option<string> Param = new(
        $"--{ParamName}"
    )
    {
        Description = "The MySQL parameter to be accessed.",
        Required = true
    };

    public static readonly Option<string> Value = new(
        $"--{ValueName}"
    )
    {
        Description = "The value to set for the MySQL parameter.",
        Required = true
    };
}
