// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Cosmos.Options.Item;

public sealed class ItemVectorSearchOptions : ISubscriptionOption
{
    [Option(Description = "The document property containing the vector embedding (e.g., 'embedding' or 'metadata.vector'). The container must have a vector index on this property.")]
    public required string VectorProperty { get; set; }

    [Option(Description = CosmosOptionDescriptions.PropertiesToSelect)]
    public string? PropertiesToSelect { get; set; }

    [Option(Description = CosmosOptionDescriptions.Count)]
    public int? Count { get; set; }

    [Option(Description = "Free-form text to embed via Azure OpenAI before searching.")]
    public required string SearchText { get; set; }

    [Option(Name = "openai-endpoint", Description = "Azure OpenAI endpoint (e.g., 'https://my-endpoint.openai.azure.com/') used to generate the embedding from --search-text.")]
    public required string OpenAIEndpoint { get; set; }

    [Option(Description = "Name of the Azure OpenAI embedding deployment (e.g., 'text-embedding-3-small') used to generate the embedding from --search-text.")]
    public required string EmbeddingDeployment { get; set; }

    [Option(Description = "Optional embedding dimensions to request from the model (only honored by models that support custom dimensions, e.g., text-embedding-3-*).")]
    public int? EmbeddingDimensions { get; set; }

    [Option(Description = CosmosOptionDescriptions.Container)]
    public required string Container { get; set; }

    [Option(Description = CosmosOptionDescriptions.Database)]
    public required string Database { get; set; }

    [Option(Description = CosmosOptionDescriptions.Account)]
    public required string Account { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(Description = OptionDescriptions.AuthMethod)]
    public AuthMethod? AuthMethod { get; set; }
}
