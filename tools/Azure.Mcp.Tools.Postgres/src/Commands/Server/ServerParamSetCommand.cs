// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Options.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres.Commands.Server;

public sealed class ServerParamSetCommand(ILogger<ServerParamSetCommand> logger) : BaseServerCommand<ServerParamSetOptions>(logger)
{
    private const string CommandTitle = "Set PostgreSQL Server Parameter";
    public override string Name => "set";

    public override string Description =>
        "Sets a specific parameter of a PostgreSQL server to a certain value.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(PostgresOptionDefinitions.Param);
        command.Options.Add(PostgresOptionDefinitions.Value);
    }

    protected override ServerParamSetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Param = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Param.Name);
        options.Value = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Value.Name);
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
            var result = await pgService.SetServerParameterAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Param!, options.Value!);
            context.Response.Results = !string.IsNullOrEmpty(result) ?
                ResponseResult.Create(new(result, options.Param!, options.Value!), PostgresJsonContext.Default.ServerParamSetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred setting the parameter.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record ServerParamSetCommandResult(string Message, string Parameter, string Value);
}
