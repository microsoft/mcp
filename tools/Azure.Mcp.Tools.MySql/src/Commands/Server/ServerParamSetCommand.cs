// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.MySql.Options.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
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
public sealed class ServerParamSetCommand(ILogger<ServerParamSetCommand> logger, IMySqlService mysqlService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ServerParamSetOptions, ServerParamSetCommand.ServerParamSetCommandResult>(subscriptionResolver)
{
    private readonly IMySqlService _mysqlService = mysqlService;
    private readonly ILogger<ServerParamSetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ServerParamSetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mysqlService.SetServerParameterAsync(options.Subscription!, options.ResourceGroup, options.Server, options.Param, options.Value, cancellationToken);
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

    public sealed record ServerParamSetCommandResult(string Parameter, string Value);
}
