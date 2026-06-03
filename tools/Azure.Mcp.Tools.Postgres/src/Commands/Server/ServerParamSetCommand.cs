// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Options.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Azure.Mcp.Tools.Postgres.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Postgres.Commands.Server;

[CommandMetadata(
    Id = "2134621b-518f-48ac-a66a-82c40fcb58bb",
    Name = "set",
    Title = "Set PostgreSQL Server Parameter",
    Description = "Configures PostgreSQL server settings including replication, connection limits, and other parameters.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerParamSetCommand(IPostgresService postgresService, ILogger<ServerParamSetCommand> logger) : BaseServerCommand<ServerParamSetOptions>(logger)
{
    private readonly IPostgresService _postgresService = postgresService;

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            ServerParameterValidator.EnsureParameterAllowed(options.Param);

            var result = await _postgresService.SetServerParameterAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Param!, options.Value!, cancellationToken);
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
