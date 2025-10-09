// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Tables.Options;

public static class TablesOptionDefinitions
{
    public const string StorageAccountName = "storage-account";
    public const string CosmosDbAccountName = "cosmosdb-account";

    public static readonly Option<string> Account = new($"--{StorageAccountName}")
    {
        Description = "The name of the Azure Storage account (e.g., 'mystorageaccount').",
    };

    public static readonly Option<string> CosmosDbAccount = new($"--{CosmosDbAccount}")
    {
        Description = "The name of the Cosmos DB account (e.g., 'mycosmosdbaccount').",
    };
}
