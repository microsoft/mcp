// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Cosmos.Commands;

public sealed class CosmosListCommand(ILogger<CosmosListCommand> logger) : SubscriptionCommand<CosmosListOptions>()
{
    private const string CommandTitle = "List Cosmos DB Resources";
    private readonly ILogger<CosmosListCommand> _logger = logger;

    public override string Id => "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

    public override string Name => "list";

    public override string Description =>
        "List Cosmos DB accounts, databases, or containers. Returns all accounts in the subscription by default. " +
        "Specify --account to list databases in that account, or --account and --database to list containers in a specific database.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CosmosOptionDefinitions.AccountOptional);
        command.Options.Add(CosmosOptionDefinitions.DatabaseOptional);
    }

    protected override CosmosListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Account = parseResult.GetValueOrDefault<string?>(CosmosOptionDefinitions.AccountOptional.Name);
        options.Database = parseResult.GetValueOrDefault<string?>(CosmosOptionDefinitions.DatabaseOptional.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        // Validate that --account is provided when --database is specified
        if (!string.IsNullOrEmpty(options.Database) && string.IsNullOrEmpty(options.Account))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "The --account parameter is required when --database is specified.";
            return context.Response;
        }

        try
        {
            var cosmosService = context.GetService<ICosmosService>() ?? throw new InvalidOperationException("Cosmos DB service is not available.");

            if (!string.IsNullOrEmpty(options.Database))
            {
                // List containers in the specified database
                var containers = await cosmosService.ListContainers(
                    options.Account!,
                    options.Database!,
                    options.Subscription!,
                    options.AuthMethod ?? AuthMethod.Credential,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new CosmosListCommandResult(null, null, containers ?? []),
                    CosmosJsonContext.Default.CosmosListCommandResult);
            }
            else if (!string.IsNullOrEmpty(options.Account))
            {
                // List databases in the specified account
                var databases = await cosmosService.ListDatabases(
                    options.Account!,
                    options.Subscription!,
                    options.AuthMethod ?? AuthMethod.Credential,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new CosmosListCommandResult(null, databases ?? [], null),
                    CosmosJsonContext.Default.CosmosListCommandResult);
            }
            else
            {
                // List all accounts in the subscription
                var accounts = await cosmosService.GetCosmosAccounts(
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new CosmosListCommandResult(accounts ?? [], null, null),
                    CosmosJsonContext.Default.CosmosListCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
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

    internal record CosmosListCommandResult(List<string>? Accounts, List<string>? Databases, IReadOnlyList<string>? Containers);
}
