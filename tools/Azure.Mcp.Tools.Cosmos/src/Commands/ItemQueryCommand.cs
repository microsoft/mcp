// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Azure.Mcp.Tools.Cosmos.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands;

[CommandMetadata(
    Id = "5c19a92a-4e0c-44dc-b1e7-5560a0d277b5",
    Name = "query",
    Title = "Query Cosmos DB Container",
    Description = "List items from a Cosmos DB container by specifying the account name, database name, and container name, optionally providing a custom SQL query to filter results.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ItemQueryCommand(ILogger<ItemQueryCommand> logger, ICosmosService cosmosService, ISubscriptionResolver subscriptionResolver)
    : BaseCosmosCommand<ItemQueryOptions, ItemQueryCommand.ItemQueryCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ItemQueryCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;
    private const string DefaultQuery = "SELECT * FROM c";

    public override void ValidateOptions(ItemQueryOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.Query != null)
        {
            var result = CosmosQueryValidator.EnsureReadOnlySelect(options.Query);
            if (!string.IsNullOrEmpty(result))
            {
                validationResult.Errors.Add(result);
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ItemQueryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var queryToRun = options.Query ?? DefaultQuery;

            var items = await _cosmosService.QueryItems(
                options.Account,
                options.Database,
                options.Container,
                queryToRun,
                options.Subscription!,
                options.AuthMethod ?? AuthMethod.Credential,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(items ?? []), CosmosJsonContext.Default.ItemQueryCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred querying container. Account: {Account}, Database: {Database},"
                + " Container: {Container}", options.Account, options.Database, options.Container);

            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ItemQueryCommandResult(List<JsonElement> Items);
}
