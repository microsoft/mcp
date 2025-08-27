// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Options.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres.Commands.Server;

public sealed class ServerParamGetCommand(ILogger<ServerParamGetCommand> logger) : BaseServerCommand<ServerParamGetOptions>(logger)
{
    private const string CommandTitle = "Get PostgreSQL Server Parameter";
    private readonly Option<string> _paramOption = PostgresOptionDefinitions.Param;
    public override string Name => "param";

    public override string Description =>
        "Retrieves a specific parameter of a PostgreSQL server.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_paramOption);
    }

    protected override ServerParamGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Param = parseResult.GetValue(_paramOption);
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
            IPostgresService pgService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");
            var parameterValue = await pgService.GetServerParameterAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Param!);
            context.Response.Results = parameterValue?.Length > 0 ?
                ResponseResult.Create(
                    new ServerParamGetCommandResult(parameterValue),
                    PostgresJsonContext.Default.ServerParamGetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving the parameter.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record ServerParamGetCommandResult(string ParameterValue);
}
