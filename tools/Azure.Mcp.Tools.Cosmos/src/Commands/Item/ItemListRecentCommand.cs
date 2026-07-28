// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Cosmos.Options.Item;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands.Item;

[CommandMetadata(
    Id = "9a1b1c2d-3e4f-4a5b-9c6d-7e8f9a0b1c2d",
    Name = "list-recent",
    Title = "List Recent Cosmos DB Documents",
    Description = "Retrieve the most recently modified documents from a Cosmos DB container, ordered by the system timestamp (_ts) in descending order. Use the --count option to control how many documents are returned (1-20, default is 10).",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ItemListRecentCommand(ILogger<ItemListRecentCommand> logger, ICosmosService cosmosService, ISubscriptionResolver subscriptionResolver)
    : BaseCosmosCommand<ItemListRecentOptions, ItemListRecentCommand.ItemListRecentCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ItemListRecentCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    public override void ValidateOptions(ItemListRecentOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.Count != null && (options.Count < 1 || options.Count > 20))
        {
            validationResult.Errors.Add("--count must be between 1 and 20.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemListRecentOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _cosmosService.GetRecentItems(
                options.Account,
                options.Database,
                options.Container,
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

    public sealed record ItemListRecentCommandResult(List<JsonElement> Items);
}
