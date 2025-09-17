// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.MySql.Json;
using Azure.Mcp.Tools.MySql.Options;
using Azure.Mcp.Tools.MySql.Options.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.MySql.Commands.Server;

public sealed class ServerParamSetCommand(ILogger<ServerParamSetCommand> logger) : BaseServerCommand<ServerParamSetOptions>(logger)
{
    private const string CommandTitle = "Set MySQL Server Parameter";

    public override string Name => "set";

    public override string Description => "Sets/updates a MySQL server configuration parameter to a new value to optimize performance, security, or operational behavior. This command enables fine-tuned configuration management with validation to ensure parameter changes are compatible with the server's current state and constraints.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

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
            string result = await mysqlService.SetServerParameterAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Param!, options.Value!);
            context.Response.Results = !string.IsNullOrEmpty(result) ?
                ResponseResult.Create(
                    new ServerParamSetCommandResult(options.Param!, result),
                    MySqlJsonContext.Default.ServerParamSetCommandResult) :
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
