// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Replication;

[CommandMetadata(
    Id = "27c0ec57-d7f8-4b02-9c34-51cd0ee761f1",
    Name = "approve",
    Description = "Authorize source volume replication for an Azure NetApp Files volume.",
    Title = "Approve NetApp Files Replication",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationApproveCommand(ILogger<ReplicationApproveCommand> logger, INetAppFilesService netAppFilesService) : ReplicationCommandBase<ReplicationApproveOptions>()
{
    private readonly ILogger<ReplicationApproveCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RegisterReplicationOptions(command, includeNoWait: true);
        command.Options.Add(NetAppFilesOptionDefinitions.RemoteVolumeResourceId);
    }

    protected override ReplicationApproveOptions BindOptions(ParseResult parseResult)
    {
        var options = BindReplicationOptions(parseResult, base.BindOptions(parseResult));
        options.RemoteVolumeResourceId = parseResult.GetValueOrDefault<string>(NetAppFilesOptionDefinitions.RemoteVolumeResourceId.Name);
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
            ValidateVolumeTarget(options);
            ValidateUnsupportedActionOptions(options);

            if (string.IsNullOrWhiteSpace(options.RemoteVolumeResourceId))
            {
                throw new ArgumentException("Provide --remoteVolumeResourceId for replication approval.");
            }

            var result = await _netAppFilesService.ApproveReplication(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.RemoteVolumeResourceId, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ReplicationOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving NetApp Files replication. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
}