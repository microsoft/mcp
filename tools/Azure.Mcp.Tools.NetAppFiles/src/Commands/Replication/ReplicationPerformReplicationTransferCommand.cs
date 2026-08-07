// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Replication;

[CommandMetadata(
    Id = "5c57d0c0-7a86-4cf3-93d7-9b0e147bb47d",
    Name = "perform-replication-transfer",
    Description = "Perform an ad-hoc Azure NetApp Files replication transfer for a migration volume.",
    Title = "Perform NetApp Files Replication Transfer",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationPerformReplicationTransferCommand(ILogger<ReplicationPerformReplicationTransferCommand> logger, INetAppFilesService netAppFilesService) : ReplicationCommandBase<ReplicationActionOptions>()
{
    private readonly ILogger<ReplicationPerformReplicationTransferCommand> _logger = logger;
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
            var result = await _netAppFilesService.PerformReplicationTransfer(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ReplicationOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing NetApp Files replication transfer. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}