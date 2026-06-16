// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.Cosmos.Models;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Options.Item;
using Azure.Mcp.Tools.Cosmos.Services;
using Azure.Mcp.Tools.Cosmos.Validation;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
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
public sealed class ItemVectorSearchCommand(ILogger<ItemVectorSearchCommand> logger, ICosmosService cosmosService)
    : BaseContainerCommand<ItemVectorSearchOptions>()
{
    private readonly ILogger<ItemVectorSearchCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.VectorProperty);
        command.Options.Add(CosmosOptionDefinitions.PropertiesToSelect);
        command.Options.Add(CosmosOptionDefinitions.Count);
        command.Options.Add(CosmosOptionDefinitions.SearchText);
        command.Options.Add(CosmosOptionDefinitions.OpenAIEndpoint);
        command.Options.Add(CosmosOptionDefinitions.EmbeddingDeployment);
        command.Options.Add(CosmosOptionDefinitions.EmbeddingDimensions);

        command.Validators.Add(result =>
        {
            var vectorProperty = result.GetValueOrDefault<string>(CosmosOptionDefinitions.VectorProperty.Name);
            if (!PropertyValidator.IsValid(vectorProperty))
            {
                result.AddError("--vector-property must be a dot-delimited identifier (letters, digits, and underscores only).");
            }

            var propertiesToSelect = result.GetValueOrDefault<string>(CosmosOptionDefinitions.PropertiesToSelect.Name);
            if (!string.IsNullOrWhiteSpace(propertiesToSelect))
            {
                if (propertiesToSelect.Contains('*'))
                {
                    result.AddError("--properties-to-select must be a comma-separated list of explicit property names (no '*' wildcards). Omit the option to return all properties except the vector.");
                }
                else
                {
                    var invalidProperties = propertiesToSelect
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Where(prop => !PropertyValidator.IsValid(prop))
                        .ToList();

                    if (invalidProperties.Count > 0)
                    {
                        result.AddError($"--properties-to-select contains invalid property name(s) '{string.Join("', '", invalidProperties)}'. Use letters, digits, and underscores only.");
                    }
                }
            }

            var count = result.GetValueOrDefault<int>(CosmosOptionDefinitions.Count.Name);
            if (count < 1 || count > 20)
            {
                result.AddError("--count must be between 1 and 20.");
            }

            var openAIEndpoint = result.GetValueOrDefault<string>(CosmosOptionDefinitions.OpenAIEndpoint.Name);
            ValidateOpenAIEndpoint(openAIEndpoint, result);
        });
    }

    private static readonly string[] s_openAIEndpointServiceTypes = ["azure-openai", "foundry"];

    private static readonly ArmEnvironment[] s_openAIEndpointClouds =
        [ArmEnvironment.AzurePublicCloud, ArmEnvironment.AzureChina, ArmEnvironment.AzureGovernment, ArmEnvironment.AzureGermany];

    private static void ValidateOpenAIEndpoint(string? endpoint, System.CommandLine.Parsing.CommandResult result)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            result.AddError("--openai-endpoint is required.");
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

        result.AddError(
            $"The provided Azure OpenAI endpoint is not a trusted Azure OpenAI, Cognitive Services, or AI Foundry endpoint for the configured Azure cloud. The value '{endpoint}' is not allowed.");
    }

    protected override ItemVectorSearchOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VectorProperty = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.VectorProperty.Name);
        options.PropertiesToSelect = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.PropertiesToSelect.Name);
        options.Count = parseResult.GetValueOrDefault<int>(CosmosOptionDefinitions.Count.Name);
        options.SearchText = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.SearchText.Name);
        options.OpenAIEndpoint = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.OpenAIEndpoint.Name);
        options.EmbeddingDeployment = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.EmbeddingDeployment.Name);
        options.EmbeddingDimensions = parseResult.GetValueOrDefault<int?>(CosmosOptionDefinitions.EmbeddingDimensions.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var propertiesToSelect = string.IsNullOrWhiteSpace(options.PropertiesToSelect)
                ? null
                : options.PropertiesToSelect
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var embedding = await _cosmosService.GenerateEmbedding(
                options.SearchText!,
                new EmbeddingRequest(options.OpenAIEndpoint!, options.EmbeddingDeployment!, options.EmbeddingDimensions),
                options.Tenant,
                cancellationToken);

            var items = await _cosmosService.VectorSearch(
                options.Account!,
                options.Database!,
                options.Container!,
                options.VectorProperty!,
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

    internal record ItemVectorSearchCommandResult(List<JsonElement> Items);
}
