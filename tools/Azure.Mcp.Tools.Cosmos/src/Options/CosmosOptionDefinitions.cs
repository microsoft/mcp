// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Cosmos.Options;

public static class CosmosOptionDefinitions
{
    public const string AccountName = "account";
    public const string DatabaseName = "database";
    public const string ContainerName = "container";
    public const string QueryText = "query";
    public const string CountName = "count";
    public const string SampleSizeName = "sample-size";
    public const string ItemIdName = "id";
    public const string PartitionKeyName = "partition-key";
    public const string SearchPropertyName = "search-property";
    public const string SearchPhraseName = "search-phrase";
    public const string VectorPropertyName = "vector-property";
    public const string PropertiesToSelectName = "properties-to-select";
    public const string SearchTextName = "search-text";
    public const string OpenAIEndpointName = "openai-endpoint";
    public const string EmbeddingDeploymentName = "embedding-deployment";
    public const string EmbeddingDimensionsName = "embedding-dimensions";

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
        DefaultValueFactory = _ => "SELECT * FROM c"
    };

    public static readonly Option<int> Count = new(
        $"--{CountName}"
    )
    {
        Description = "Maximum number of documents to return (1-20). Defaults to 10.",
        DefaultValueFactory = _ => 10
    };

    public static readonly Option<int> SampleSize = new(
        $"--{SampleSizeName}"
    )
    {
        Description = "Number of documents to sample for schema inference (1-20). Defaults to 10.",
        DefaultValueFactory = _ => 10
    };

    public static readonly Option<string> ItemId = new(
        $"--{ItemIdName}"
    )
    {
        Description = "The id of the document to retrieve.",
        Required = true
    };

    public static readonly Option<string?> PartitionKey = new(
        $"--{PartitionKeyName}"
    )
    {
        Description = "Optional partition key value for the document. When provided, the query is scoped to a single partition (cheaper than a cross-partition fan-out)."
    };

    public static readonly Option<string> SearchProperty = new(
        $"--{SearchPropertyName}"
    )
    {
        Description = "The document property to search. Supports dot notation (e.g., 'name' or 'profile.name'). Allowed characters: letters, digits, and underscores.",
        Required = true
    };

    public static readonly Option<string> SearchPhrase = new(
        $"--{SearchPhraseName}"
    )
    {
        Description = "The phrase to search for. Passed as a parameterized value to a Cosmos DB FullTextContains query. The container must have a full-text index on the property.",
        Required = true
    };

    public static readonly Option<string> VectorProperty = new(
        $"--{VectorPropertyName}"
    )
    {
        Description = "The document property containing the vector embedding (e.g., 'embedding' or 'metadata.vector'). The container must have a vector index on this property.",
        Required = true
    };

    public static readonly Option<string> PropertiesToSelect = new(
        $"--{PropertiesToSelectName}"
    )
    {
        Description = "Comma-separated list of properties to project in the result (e.g., 'id,title,metadata.author'). Wildcards ('*') are not supported in this list; omit this option to return all properties."
    };

    public static readonly Option<string> SearchText = new(
        $"--{SearchTextName}"
    )
    {
        Description = "Free-form text to embed via Azure OpenAI before searching.",
        Required = true
    };

    public static readonly Option<string> OpenAIEndpoint = new(
        $"--{OpenAIEndpointName}"
    )
    {
        Description = "Azure OpenAI endpoint (e.g., 'https://my-endpoint.openai.azure.com/') used to generate the embedding from --search-text.",
        Required = true
    };

    public static readonly Option<string> EmbeddingDeployment = new(
        $"--{EmbeddingDeploymentName}"
    )
    {
        Description = "Name of the Azure OpenAI embedding deployment (e.g., 'text-embedding-3-small') used to generate the embedding from --search-text.",
        Required = true
    };

    public static readonly Option<int?> EmbeddingDimensions = new(
        $"--{EmbeddingDimensionsName}"
    )
    {
        Description = "Optional embedding dimensions to request from the model (only honored by models that support custom dimensions, e.g., text-embedding-3-*)."
    };
}
