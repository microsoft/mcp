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
    Id = "da863d2c-0d28-4bff-9f23-75c8903ba24f",
    Name = "suspend",
    Description = "Suspend or break Azure NetApp Files replication on the destination volume.",
    Title = "Suspend NetApp Files Replication",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationSuspendCommand(ILogger<ReplicationSuspendCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver) 
    : SubscriptionCommand<ReplicationSuspendOptions, ReplicationSuspendCommand.ReplicationSuspendCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ReplicationSuspendCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ReplicationSuspendOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ReplicationCommandHelpers.ValidateVolumeTarget(options);
            ReplicationCommandHelpers.ValidateUnsupportedActionOptions(options);
            var result = await _netAppFilesService.SuspendReplication(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.ForceBreakReplication, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ReplicationOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending NetApp Files replication. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ReplicationSuspendCommandResult([property: JsonPropertyName("replicationSuspendResult")] ReplicationOperationResult ReplicationOperationResult);
}