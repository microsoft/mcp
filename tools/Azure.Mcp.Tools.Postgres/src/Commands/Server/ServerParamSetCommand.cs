// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Postgres.Options.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Azure.Mcp.Tools.Postgres.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
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
public sealed class ServerParamSetCommand(IPostgresService postgresService, ILogger<ServerParamSetCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ServerParamSetOptions, ServerParamSetCommand.ServerParamSetCommandResult>(subscriptionResolver)
{
    private readonly IPostgresService _postgresService = postgresService;
    private readonly ILogger<ServerParamSetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServerParamSetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ServerParameterValidator.EnsureParameterAllowed(options.Param);

            var result = await _postgresService.SetServerParameterAsync(options.Subscription!, options.ResourceGroup, options.User, options.Server, options.Param, options.Value, options.Tenant, options.RetryPolicy, cancellationToken);
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

    public sealed record ServerParamSetCommandResult(string Message, string Parameter, string Value);
}
