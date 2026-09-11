// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.NetAppFiles.Options.Replication;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Replication;

[CommandMetadata(
    Id = "d23685f7-2a68-4f70-bfcc-f4a0e8d1c57a",
    Name = "finalize-external-replication",
    Description = "Finalize external Azure NetApp Files replication migration.",
    Title = "Finalize External NetApp Files Replication",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationFinalizeExternalReplicationCommand(ILogger<ReplicationFinalizeExternalReplicationCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver) 
    : SubscriptionCommand<ReplicationActionOptions, ReplicationFinalizeExternalReplicationCommand.ReplicationFinalizeExternalReplicationCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ReplicationFinalizeExternalReplicationCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ReplicationActionOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ReplicationCommandHelpers.ValidateVolumeTarget(options);
            ReplicationCommandHelpers.ValidateUnsupportedActionOptions(options);
            var result = await _netAppFilesService.FinalizeExternalReplication(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ReplicationOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finalizing external NetApp Files replication. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }
    public record ReplicationFinalizeExternalReplicationCommandResult([property: JsonPropertyName("replicationFinalizeExternalReplicationResult")] ReplicationOperationResult ReplicationOperationResult);
}