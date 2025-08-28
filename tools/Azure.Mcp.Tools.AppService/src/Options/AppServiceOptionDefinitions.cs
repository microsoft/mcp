// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AppService.Options;

public static class AppServiceOptionDefinitions
{
    public const string AppName = "app-name";
    public const string DatabaseType = "database-type";
    public const string DatabaseServer = "database-server";
    public const string DatabaseName = "database-name";
    public const string ConnectionString = "connection-string";

    public static readonly Option<string> AppServiceName = new(
        $"--{AppName}",
        "The name of the Azure App Service."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> DatabaseTypeOption = new(
        $"--{DatabaseType}",
        "The type of database (e.g., SqlServer, MySQL, PostgreSQL, CosmosDB)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> DatabaseServerOption = new(
        $"--{DatabaseServer}",
        "The server name or endpoint for the database."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> DatabaseNameOption = new(
        $"--{DatabaseName}",
        "The name of the database."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> ConnectionStringOption = new(
        $"--{ConnectionString}",
        "The connection string for the database. If not provided, a default will be generated."
    )
    {
        IsRequired = false
    };
}
