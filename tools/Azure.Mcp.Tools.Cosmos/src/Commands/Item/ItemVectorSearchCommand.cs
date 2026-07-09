// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Cosmos.Options.Item;
using Azure.Mcp.Tools.Cosmos.Services;
using Azure.Mcp.Tools.Cosmos.Validation;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.Item;

[CommandMetadata(
    Id = "5e6f7a8b-9c0d-4e1f-a2b3-c4d5e6f7a8b9",
    Name = "vector-search",
    Title = "Vector Search Cosmos DB Documents",
    Description = "Retrieve the TOP N documents in a Cosmos DB container most similar to a given --search-text using the Cosmos VectorDistance function on the provided --vector-property. The tool first calls an Azure OpenAI embedding deployment (--openai-endpoint and --embedding-deployment) to convert --search-text into a query vector; optionally pass --embedding-dimensions to request a specific length for models that support custom dimensions (e.g., text-embedding-3-small / text-embedding-3-large). Each returned document includes a `_score` field that represents the server-side computed similarity. Requires a Cosmos DB vector index on --vector-property. Use --count to control how many documents are returned (1-20, default is 10). Optionally pass --properties-to-select to project specific fields; when omitted the full document is returned with --vector-property stripped, since a typical 1536-dim embedding adds ~30 KB / ~10K tokens per hit.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ItemVectorSearchCommand(ILogger<ItemVectorSearchCommand> logger, ICosmosService cosmosService, ISubscriptionResolver subscriptionResolver)
    : BaseCosmosCommand<ItemVectorSearchOptions, ItemVectorSearchCommand.ItemVectorSearchCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ItemVectorSearchCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    public override void ValidateOptions(ItemVectorSearchOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!PropertyValidator.IsValid(options.VectorProperty))
        {
            validationResult.Errors.Add("--vector-property must be a dot-delimited identifier (letters, digits, and underscores only).");
        }

        if (!string.IsNullOrWhiteSpace(options.PropertiesToSelect))
        {
            if (options.PropertiesToSelect.Contains('*'))
            {
                validationResult.Errors.Add("--properties-to-select must be a comma-separated list of explicit property names (no '*' wildcards). Omit the option to return all properties except the vector.");
            }
            else
            {
                var invalidProperties = options.PropertiesToSelect
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(prop => !PropertyValidator.IsValid(prop))
                    .ToList();

                if (invalidProperties.Count > 0)
                {
                    validationResult.Errors.Add($"--properties-to-select contains invalid property name(s) '{string.Join("', '", invalidProperties)}'. Use letters, digits, and underscores only.");
                }
            }
        }

        if (options.Count != null && (options.Count < 1 || options.Count > 20))
        {
            validationResult.Errors.Add("--count must be between 1 and 20.");
        }

        ValidateOpenAIEndpoint(options.OpenAIEndpoint, validationResult);
    }

    private static readonly string[] s_openAIEndpointServiceTypes = ["azure-openai", "foundry"];

    private static readonly ArmEnvironment[] s_openAIEndpointClouds =
        [ArmEnvironment.AzurePublicCloud, ArmEnvironment.AzureChina, ArmEnvironment.AzureGovernment, ArmEnvironment.AzureGermany];

    private static void ValidateOpenAIEndpoint(string? endpoint, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            validationResult.Errors.Add("--openai-endpoint is required.");
            return;
        }

        // The configured Azure cloud is not available during option validation, so accept the
        // endpoint if it is valid for any supported cloud. The service performs the authoritative,
        // cloud-aware check before constructing the authenticated client.
        foreach (var cloud in s_openAIEndpointClouds)
        {
            foreach (var serviceType in s_openAIEndpointServiceTypes)
            {
                try
                {
                    EndpointValidator.ValidateAzureServiceEndpoint(endpoint, serviceType, cloud);
                    return;
                }
                catch (Exception ex) when (ex is SecurityException or ArgumentException)
                {
                    // Ignored. Will reach error message if endpoint is not valid for any supported cloud.
                }
            }
        }

        validationResult.Errors.Add(
            $"The provided Azure OpenAI endpoint is not a trusted Azure OpenAI, Cognitive Services, or AI Foundry endpoint for the configured Azure cloud. The value '{endpoint}' is not allowed.");
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemVectorSearchOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var propertiesToSelect = string.IsNullOrWhiteSpace(options.PropertiesToSelect)
                ? null
                : options.PropertiesToSelect
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var embedding = await _cosmosService.GenerateEmbedding(
                options.SearchText,
                new(options.OpenAIEndpoint, options.EmbeddingDeployment, options.EmbeddingDimensions),
                options.Tenant,
                cancellationToken);

            var items = await _cosmosService.VectorSearch(
                options.Account,
                options.Database,
                options.Container,
                options.VectorProperty,
                propertiesToSelect,
                embedding,
                options.Count ?? 10,
                options.Subscription!,
                options.AuthMethod ?? AuthMethod.Credential,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ItemVectorSearchCommandResult(items ?? []),
                CosmosJsonContext.Default.ItemVectorSearchCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Account: {Account}, Database: {Database}, Container: {Container}, VectorProperty: {VectorProperty}",
                Name, options.Account, options.Database, options.Container, options.VectorProperty);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ItemVectorSearchCommandResult(List<JsonElement> Items);
}
