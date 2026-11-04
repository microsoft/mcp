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
    Description = "Retrieve one page of up to 10 documents from a Cosmos DB container, ordered by the system timestamp (_ts) in descending order, most recently modified first. The page size is fixed; Cosmos DB may return fewer items or an empty page. Pass nextCursor unchanged as --cursor with the same account, database, and container to retrieve another page.",
    OperationPlane = ToolOperationPlane.Data,
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemListRecentOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _cosmosService.GetRecentItems(
                options.Account,
                options.Database,
                options.Container,
                options.Subscription!,
                options.Cursor,
                options.AuthMethod ?? AuthMethod.Credential,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ItemListRecentCommandResult(items.Items ?? [], items.NextCursor),
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

    public sealed record ItemListRecentCommandResult(List<JsonElement> Items, string? NextCursor = null);
}
