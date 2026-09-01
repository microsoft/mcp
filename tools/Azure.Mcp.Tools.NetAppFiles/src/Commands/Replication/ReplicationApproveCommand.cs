// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Options;
using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;

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
public sealed class ReplicationApproveCommand(ILogger<ReplicationApproveCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ReplicationApproveOptions, ReplicationApproveCommand.ReplicationApproveCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ReplicationApproveCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ReplicationApproveOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ReplicationCommandHelpers.ValidateVolumeTarget(options);
            ReplicationCommandHelpers.ValidateUnsupportedActionOptions(options);

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

    public record ReplicationApproveCommandResult([property: JsonPropertyName("replicationApproveResult")] ReplicationOperationResult ReplicationOperationResult);
}