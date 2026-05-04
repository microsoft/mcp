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
    Id = "8d086e44-8c8a-4649-a282-38f775704595",
    Name = "set",
    Title = "Set MySQL Server Parameter",
    Description = "Sets/updates a single MySQL server configuration setting/parameter.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerParamSetCommand(ILogger<ServerParamSetCommand> logger, IMySqlService mysqlService) : BaseServerCommand<ServerParamSetOptions>(logger)
{
    private readonly IMySqlService _mysqlService = mysqlService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MySqlOptionDefinitions.Param);
        command.Options.Add(MySqlOptionDefinitions.Value);
    }

    protected override ServerParamSetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Param = parseResult.GetValueOrDefault<string>(MySqlOptionDefinitions.Param.Name);
        options.Value = parseResult.GetValueOrDefault<string>(MySqlOptionDefinitions.Value.Name);
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
            var result = await _mysqlService.SetServerParameterAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Param!, options.Value!, cancellationToken);
            context.Response.Results = !string.IsNullOrEmpty(result) ?
                ResponseResult.Create(new(options.Param!, result), MySqlJsonContext.Default.ServerParamSetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred setting server parameter.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record ServerParamSetCommandResult(string Parameter, string Value);
}
