// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Cosmos.Commands;

public sealed class ItemQueryCommand(ILogger<ItemQueryCommand> logger) : BaseContainerCommand<ItemQueryOptions>()
{
    private const string CommandTitle = "Query Cosmos DB Container";
    private readonly ILogger<ItemQueryCommand> _logger = logger;
    private const string DefaultQuery = "SELECT * FROM c";

    private readonly Option<string> _queryOption = CosmosOptionDefinitions.Query;

    public override string Name => "query";

    public override string Description =>
        $"""
        Execute a SQL query against items in a Cosmos DB container. Requires {CosmosOptionDefinitions.AccountName},
        {CosmosOptionDefinitions.DatabaseName}, and {CosmosOptionDefinitions.ContainerName}.
        The {CosmosOptionDefinitions.QueryText} parameter accepts SQL query syntax. Results are returned as a
        JSON array of documents.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_queryOption);
    }

    protected override ItemQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValueForOption(_queryOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

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

            context.Response.Results = items?.Count > 0 ?
                ResponseResult.Create(new ItemQueryCommandResult(items), CosmosJsonContext.Default.ItemQueryCommandResult) :
                null;
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
