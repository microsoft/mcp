// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Cosmos.Models;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Options.Item;
using Azure.Mcp.Tools.Cosmos.Services;
using Azure.Mcp.Tools.Cosmos.Validation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.Item;

[CommandMetadata(
    Id = "5e6f7a8b-9c0d-4e1f-a2b3-c4d5e6f7a8b9",
    Name = "vector-search",
    Title = "Vector Search Cosmos DB Documents",
    Description = "Perform a vector similarity search on Cosmos DB. Supplies --search-text to an Azure OpenAI embedding deployment (--openai-endpoint and --embedding-deployment) to generate the query vector. The container must have a vector index on --vector-property.",
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
        command.Options.Add(CosmosOptionDefinitions.SelectProperties);
        command.Options.Add(CosmosOptionDefinitions.VectorSearchCount);
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

            var selectProperties = result.GetValueOrDefault<string>(CosmosOptionDefinitions.SelectProperties.Name);
            if (string.IsNullOrWhiteSpace(selectProperties) || selectProperties.Contains('*'))
            {
                result.AddError("--select-properties must be a comma-separated list of explicit property names (no '*' wildcards).");
            }
            else
            {
                foreach (var prop in selectProperties.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!PropertyValidator.IsValid(prop))
                    {
                        result.AddError($"--select-properties contains an invalid property '{prop}'. Use letters, digits, and underscores only.");
                        break;
                    }
                }
            }

            var count = result.GetValueOrDefault<int>(CosmosOptionDefinitions.VectorSearchCount.Name);
            if (count < 1 || count > 50)
            {
                result.AddError("--count must be between 1 and 50.");
            }
        });
    }

    protected override ItemVectorSearchOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.VectorProperty = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.VectorProperty.Name);
        options.SelectProperties = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.SelectProperties.Name);
        options.Count = parseResult.GetValueOrDefault<int>(CosmosOptionDefinitions.VectorSearchCount.Name);
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
            var selectProperties = options.SelectProperties!
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
                selectProperties,
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

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        CosmosException cosmosEx => cosmosEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        CosmosException cosmosEx => cosmosEx.StatusCode,
        _ => base.GetStatusCode(ex)
    };

    internal record ItemVectorSearchCommandResult(List<JsonElement> Items);
}
