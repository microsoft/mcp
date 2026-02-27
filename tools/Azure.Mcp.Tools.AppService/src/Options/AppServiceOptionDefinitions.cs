// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppService.Options;

public static class AppServiceOptionDefinitions
{
    public const string AppName = "app";
    public const string DatabaseType = "database-type";
    public const string DatabaseServer = "database-server";
    public const string DatabaseName = "database";
    public const string ConnectionString = "connection-string";
    public const string AppSettingNameName = "setting-name";
    public const string AppSettingValueName = "setting-value";
    public const string AppSettingUpdateTypeName = "setting-update-type";

    public static readonly Option<string> AppServiceName = new($"--{AppName}")
    {
        Description = "The name of the Azure App Service (e.g., my-webapp).",
        Required = true
    };

    public static readonly Option<string> DatabaseTypeOption = new($"--{DatabaseType}")
    {
        Description = "The type of database (e.g., SqlServer, MySQL, PostgreSQL, CosmosDB).",
        Required = true
    };

    public static readonly Option<string> DatabaseServerOption = new($"--{DatabaseServer}")
    {
        Description = "The server name or endpoint for the database (e.g., myserver.database.windows.net).",
        Required = true
    };

    public static readonly Option<string> DatabaseNameOption = new($"--{DatabaseName}")
    {
        Description = "The name of the database to connect to (e.g., mydb).",
        Required = true
    };

    public static readonly Option<string> ConnectionStringOption = new($"--{ConnectionString}")
    {
        Description = "The connection string for the database. If not provided, a default will be generated.",
        Required = false
    };

    public static readonly Option<string> AppSettingName = new($"--{AppSettingNameName}")
    {
        Description = "The name of the application setting.",
        Required = true
    };

    public static readonly Option<string> AppSettingValue = new($"--{AppSettingValueName}")
    {
        Description = "The value of the application setting. Required for add and set update types.",
        Required = false
    };

    public static readonly Option<string> AppSettingUpdateType = new($"--{AppSettingUpdateTypeName}")
    {
        Description = "The type of update to perform on the application setting. Valid values are: add, set, delete.",
        Required = true
    };
}
