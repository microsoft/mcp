// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Replication;

[CommandMetadata(
    Id = "8d37cfbc-db6a-4480-8731-38582d5d5441",
    Name = "re-initialize",
    Description = "Re-initialize Azure NetApp Files replication on the destination volume.",
    Title = "Re-Initialize NetApp Files Replication",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationReInitializeCommand(ILogger<ReplicationReInitializeCommand> logger, INetAppFilesService netAppFilesService) : ReplicationCommandBase<ReplicationActionOptions>()
{
    private readonly ILogger<ReplicationReInitializeCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RegisterReplicationOptions(command, includeNoWait: true);
    }

    protected override ReplicationActionOptions BindOptions(ParseResult parseResult) => BindReplicationOptions(parseResult, base.BindOptions(parseResult));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);
        try
        {
            ValidateVolumeTarget(options);
            ValidateUnsupportedActionOptions(options);
            var result = await _netAppFilesService.ReInitializeReplication(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ReplicationOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error re-initializing NetApp Files replication. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}