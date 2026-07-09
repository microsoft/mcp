// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Postgres.Options.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Postgres.Commands.Server;

[CommandMetadata(
    Id = "af3a581d-ab64-4939-9765-974815d9c7be",
    Name = "get",
    Title = "Get PostgreSQL Server Parameter",
    Description = "Retrieves a specific parameter of a PostgreSQL server.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerParamGetCommand(IPostgresService postgresService, ILogger<ServerParamGetCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ServerParamGetOptions, ServerParamGetCommand.ServerParamGetCommandResult>(subscriptionResolver)
{
    private readonly IPostgresService _postgresService = postgresService;
    private readonly ILogger<ServerParamGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServerParamGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var parameterValue = await _postgresService.GetServerParameterAsync(options.Subscription!, options.ResourceGroup, options.User, options.Server, options.Param, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = parameterValue?.Length > 0 ?
                ResponseResult.Create(new(parameterValue), PostgresJsonContext.Default.ServerParamGetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving the parameter.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record ServerParamGetCommandResult(string ParameterValue);
}
