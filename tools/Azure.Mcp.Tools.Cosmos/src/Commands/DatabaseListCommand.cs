// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Cosmos.Options;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Cosmos.Commands;

public sealed class DatabaseListCommand(ILogger<DatabaseListCommand> logger) : BaseCosmosCommand<DatabaseListOptions>()
{
    private const string CommandTitle = "List Cosmos DB Databases";
    private readonly ILogger<DatabaseListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List all databases in a Cosmos DB account. This command retrieves and displays all databases available
        in the specified Cosmos DB account. Results include database names and are returned as a JSON array.
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
            var databases = await cosmosService.ListDatabases(
                options.Account!,
                options.Subscription!,
                options.AuthMethod ?? AuthMethod.Credential,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(databases ?? []), CosmosJsonContext.Default.DatabaseListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing databases. Account: {Account}.", options.Account);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record DatabaseListCommandResult(List<string> Databases);
}
