// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Cosmos.Options;

public static class CosmosOptionDefinitions
{
    public const string AccountName = "account";
    public const string DatabaseName = "database";
    public const string ContainerName = "container";
    public const string QueryText = "query";

    public static readonly Option<string> Account = new(
        $"--{AccountName}"
    )
    {
        Description = "The name of the Cosmos DB account to query (e.g., my-cosmos-account).",
        Required = true
    };

    public static readonly Option<string?> AccountOptional = new(
        $"--{AccountName}"
    )
    {
        Description = "The name of the Cosmos DB account (optional). When not specified, lists all accounts in the subscription. Specify this to list databases, or combine with --database to list containers."
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}"
    )
    {
        Description = "The name of the database to query (e.g., my-database).",
        Required = true
    };

    public static readonly Option<string?> DatabaseOptional = new(
        $"--{DatabaseName}"
    )
    {
        Description = "The name of the database (optional). Requires --account to be specified. When provided, lists containers within this database."
    };

    public static readonly Option<string> Container = new(
        $"--{ContainerName}"
    )
    {
        Description = "The name of the container to query (e.g., my-container).",
        Required = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryText}"
    )
    {
        Description = "SQL query to execute against the container. Uses Cosmos DB SQL syntax.",
        Required = false,
        DefaultValueFactory = _ => "SELECT * FROM c"
    };
}
