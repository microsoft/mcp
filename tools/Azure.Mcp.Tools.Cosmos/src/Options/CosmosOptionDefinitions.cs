// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Cosmos.Options;

internal static class CosmosOptionDescriptions
{
    internal const string Account = "The name of the Cosmos DB account to query (e.g., my-cosmos-account).";
    internal const string Database = "The name of the database to query (e.g., my-database).";
    internal const string Container = "The name of the container to query (e.g., my-container).";
    internal const string Count = "Maximum number of documents to return (1-20). Defaults to 10.";
    internal const string PropertiesToSelect = "Comma-separated list of properties to project in the result (e.g., 'id,title,metadata.author'). Wildcards ('*') are not supported in this list; omit this option to return all properties.";
}
