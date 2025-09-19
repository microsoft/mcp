// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Cosmos.Commands;

public sealed class ItemQueryCommand(ILogger<ItemQueryCommand> logger) : BaseContainerCommand<ItemQueryOptions>()
{
    private const string CommandTitle = "Query Cosmos DB Container";
    private readonly ILogger<ItemQueryCommand> _logger = logger;
    private const string DefaultQuery = "SELECT * FROM c";

    public override string Name => "query";

    public override string Description =>
        $"""
        Execute a SQL query against items in a Cosmos DB container. Requires {CosmosOptionDefinitions.AccountName},
        {CosmosOptionDefinitions.DatabaseName}, and {CosmosOptionDefinitions.ContainerName}.
        The {CosmosOptionDefinitions.QueryText} parameter accepts SQL query syntax. Results are returned as a
        JSON array of documents.
        """;

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
        command.Options.Add(CosmosOptionDefinitions.Query);
    }

    protected override ItemQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValueOrDefault<string>(CosmosOptionDefinitions.Query.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var cosmosService = context.GetService<ICosmosService>();
            var items = await cosmosService.QueryItems(
                options.Account!,
                options.Database!,
                options.Container!,
                options.Query ?? DefaultQuery,
                options.Subscription!,
                options.AuthMethod ?? AuthMethod.Credential,
                options.Tenant,
                options.RetryPolicy);

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

    internal record ItemQueryCommandResult(List<JsonElement> Items);
}
