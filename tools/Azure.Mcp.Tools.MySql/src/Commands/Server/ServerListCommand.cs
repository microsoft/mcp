// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.MySql.Json;
using Azure.Mcp.Tools.MySql.Options.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.MySql.Commands.Server;

public sealed class ServerListCommand(ILogger<ServerListCommand> logger) : BaseMySqlCommand<ServerListOptions>(logger)
{
    private const string CommandTitle = "List MySQL Servers";

    public override string Name => "list";

    public override string Description => "Discovers and lists all Azure Database for MySQL Flexible Server instances within the specified resource group. This command provides an inventory of available MySQL server resources, including their names and current status, enabling efficient server management and resource planning.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
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
            IMySqlService mysqlService = context.GetService<IMySqlService>() ?? throw new InvalidOperationException("MySQL service is not available.");
            List<string> servers = await mysqlService.ListServersAsync(options.Subscription!, options.ResourceGroup!, options.User!);
            context.Response.Results = servers?.Count > 0 ?
                ResponseResult.Create(
                    new ServerListCommandResult(servers),
                    MySqlJsonContext.Default.ServerListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing servers.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record ServerListCommandResult(List<string> Servers);
}
