// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Options.Item;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.Item;

[CommandMetadata(
    Id = "d4c1b2a3-9e8f-4d7c-86b5-1a2b3c4d5e6f",
    Name = "get",
    Title = "Get Cosmos DB Document by Id",
    Description = "Get a single Cosmos DB document by its id in the specified database and container. When a partition-key is supplied, the query is scoped to a single partition (cheaper than a cross-partition fan-out); otherwise a cross-partition query is executed.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ItemGetCommand(ILogger<ItemGetCommand> logger, ICosmosService cosmosService)
    : BaseContainerCommand<ItemGetOptions>()
{
    private readonly ILogger<ItemGetCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.ItemId);
        command.Options.Add(CosmosOptionDefinitions.PartitionKey);
    }

    protected override ItemGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Id = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.ItemId.Name);
        options.PartitionKey = parseResult.GetValueOrDefault<string?>(CosmosOptionDefinitions.PartitionKey.Name);
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
            var item = await _cosmosService.GetItem(
                options.Account!,
                options.Database!,
                options.Container!,
                options.Id!,
                options.PartitionKey,
                options.Subscription!,
                options.AuthMethod ?? AuthMethod.Credential,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ItemGetCommandResult(item),
                CosmosJsonContext.Default.ItemGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Account: {Account}, Database: {Database}, Container: {Container}",
                Name, options.Account, options.Database, options.Container);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ItemGetCommandResult(JsonElement? Item);
}
