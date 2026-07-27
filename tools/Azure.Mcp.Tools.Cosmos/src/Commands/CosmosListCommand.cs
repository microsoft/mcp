// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands;

[CommandMetadata(
    Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    Name = "list",
    Title = "List Cosmos DB Resources",
    Description = "List Cosmos DB accounts, databases, or containers. Returns all accounts in the subscription by default. Specify --account to list databases in that account, or --account and --database to list containers in a specific database.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class CosmosListCommand(ILogger<CosmosListCommand> logger, ICosmosService cosmosService, ISubscriptionResolver subscriptionResolver)
    : BaseCosmosCommand<CosmosListOptions, CosmosListCommand.CosmosListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<CosmosListCommand> _logger = logger;
    private readonly ICosmosService _cosmosService = cosmosService;

    public override void ValidateOptions(CosmosListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Validate that --account is provided when --database is specified
        if (!string.IsNullOrEmpty(options.Database) && string.IsNullOrEmpty(options.Account))
        {
            validationResult.Errors.Add("The --account parameter is required when --database is specified.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, CosmosListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(options.Database))
            {
                // List containers in the specified database
                var containers = await _cosmosService.ListContainers(
                    options.Account!,
                    options.Database,
                    options.Subscription!,
                    options.AuthMethod ?? AuthMethod.Credential,
                    options.Tenant,
                    options.ResourceGroup,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(null, null, containers ?? []),
                    CosmosJsonContext.Default.CosmosListCommandResult);
            }
            else if (!string.IsNullOrEmpty(options.Account))
            {
                // List databases in the specified account
                var databases = await _cosmosService.ListDatabases(
                    options.Account,
                    options.Subscription!,
                    options.AuthMethod ?? AuthMethod.Credential,
                    options.Tenant,
                    options.ResourceGroup,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(null, databases ?? [], null),
                    CosmosJsonContext.Default.CosmosListCommandResult);
            }
            else
            {
                // List accounts in the subscription, optionally scoped to a resource group
                var accounts = await _cosmosService.GetCosmosAccounts(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(accounts ?? [], null, null),
                    CosmosJsonContext.Default.CosmosListCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(options.ResourceGroup))
            {
                _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}, Account: {Account}, Database: {Database}.", Name, options.Subscription, options.Account, options.Database);
            }
            else
            {
                _logger.LogError(ex, "Error in {Operation}. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Account: {Account}, Database: {Database}.", Name, options.Subscription, options.ResourceGroup, options.Account, options.Database);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record CosmosListCommandResult(List<string>? Accounts, List<string>? Databases, IReadOnlyList<string>? Containers);
}
