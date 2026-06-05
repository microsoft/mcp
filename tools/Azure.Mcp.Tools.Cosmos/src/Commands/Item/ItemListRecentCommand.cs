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
    Id = "9a1b1c2d-3e4f-4a5b-9c6d-7e8f9a0b1c2d",
    Name = "list-recent",
    Title = "List Recent Cosmos DB Documents",
    Description = "Retrieve the most recently modified documents from a Cosmos DB container, ordered by the system timestamp (_ts) in descending order. Use the count to control how many documents are returned (1-20, default is 10).",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ItemListRecentCommand(ILogger<ItemListRecentCommand> logger, ICosmosService cosmosService)
    : BaseContainerCommand<ItemListRecentOptions>()
{
    private readonly ILogger<ItemListRecentCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.Count);
        command.Validators.Add(result =>
        {
            var count = result.GetValueOrDefault<int>(CosmosOptionDefinitions.Count.Name);
            if (count < 1 || count > 20)
            {
                result.AddError("--count must be between 1 and 20.");
            }
        });
    }

    protected override ItemListRecentOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Count = parseResult.GetValueOrDefault<int>(CosmosOptionDefinitions.Count.Name);
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
            var items = await _cosmosService.GetRecentItems(
                options.Account!,
                options.Database!,
                options.Container!,
                options.Count ?? 10,
                options.Subscription!,
                options.AuthMethod ?? AuthMethod.Credential,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ItemListRecentCommandResult(items ?? []),
                CosmosJsonContext.Default.ItemListRecentCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Account: {Account}, Database: {Database}, Container: {Container}",
                Name, options.Account, options.Database, options.Container);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ItemListRecentCommandResult(List<JsonElement> Items);
}
