// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.MySql.Options;
using Azure.Mcp.Tools.MySql.Options.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.MySql.Commands.Server;

[CommandMetadata(
    Id = "bae423b4-aee8-4f23-a104-e816727d183f",
    Name = "get",
    Title = "Get MySQL Server Parameter",
    Description = "Retrieves the current value of a single server configuration parameter on an Azure Database for MySQL Flexible Server. Use to inspect a setting (e.g. max_connections, wait_timeout, slow_query_log) before changing it.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerParamGetCommand(ILogger<ServerParamGetCommand> logger, IMySqlService mysqlService) : BaseServerCommand<ServerParamGetOptions>(logger)
{
    private readonly IMySqlService _mysqlService = mysqlService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MySqlOptionDefinitions.Param);
    }

    protected override ServerParamGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Param = parseResult.GetValueOrDefault<string>(MySqlOptionDefinitions.Param.Name);
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
            var paramValue = await _mysqlService.GetServerParameterAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Param!, cancellationToken);
            context.Response.Results = !string.IsNullOrEmpty(paramValue) ?
                ResponseResult.Create(new(options.Param!, paramValue), MySqlJsonContext.Default.ServerParamGetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving server parameter.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record ServerParamGetCommandResult(string Parameter, string Value);
}
