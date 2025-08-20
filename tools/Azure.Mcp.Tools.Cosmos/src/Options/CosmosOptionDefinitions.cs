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
        $"--{AccountName}",
        "The name of the Cosmos DB account to query (e.g., my-cosmos-account)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}",
        "The name of the database to query (e.g., my-database)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Container = new(
        $"--{ContainerName}",
        "The name of the container to query (e.g., my-container)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryText}",
        () => "SELECT * FROM c",
        "SQL query to execute against the container. Uses Cosmos DB SQL syntax."
    )
    {
        IsRequired = false
    };
}
