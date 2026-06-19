// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public class ItemVectorSearchOptions : ISubscriptionOption
{
    [Option("The document property containing the vector embedding (e.g., 'embedding' or 'metadata.vector'). The container must have a vector index on this property.")]
    public required string VectorProperty { get; set; }

    [Option(CosmosOptionDescriptions.PropertiesToSelect)]
    public string? PropertiesToSelect { get; set; }

    [Option(CosmosOptionDescriptions.Count)]
    public int? Count { get; set; }

    [Option("Free-form text to embed via Azure OpenAI before searching.")]
    public required string SearchText { get; set; }

    [Option(Name = "openai-endpoint", Description = "Azure OpenAI endpoint (e.g., 'https://my-endpoint.openai.azure.com/') used to generate the embedding from --search-text.")]
    public required string OpenAIEndpoint { get; set; }

    [Option("Name of the Azure OpenAI embedding deployment (e.g., 'text-embedding-3-small') used to generate the embedding from --search-text.")]
    public required string EmbeddingDeployment { get; set; }

    [Option("Optional embedding dimensions to request from the model (only honored by models that support custom dimensions, e.g., text-embedding-3-*).")]
    public int? EmbeddingDimensions { get; set; }

    [Option(CosmosOptionDescriptions.Container)]
    public required string Container { get; set; }

    [Option(CosmosOptionDescriptions.Database)]
    public required string Database { get; set; }

    [Option(CosmosOptionDescriptions.Account)]
    public required string Account { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(OptionDescriptions.AuthMethod)]
    public AuthMethod? AuthMethod { get; set; }
}
