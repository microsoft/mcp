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
    Id = "5f43637f-75bd-40a7-999d-553f666a5fc8",
    Name = "status",
    Description = "Get the runtime status of Azure NetApp Files replication for a specified volume.",
    Title = "Get NetApp Files Replication Runtime Status",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationStatusCommand(ILogger<ReplicationStatusCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ReplicationStatusOptions, ReplicationStatusCommand.ReplicationStatusCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ReplicationStatusCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;


    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ReplicationStatusOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ReplicationCommandHelpers.ValidateVolumeTarget(options);
            ReplicationCommandHelpers.ValidateUnsupportedCommonOptions(options);
            var result = await _netAppFilesService.GetReplicationStatus(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.VolumeReplicationStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting NetApp Files replication runtime status. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ReplicationStatusCommandResult([property: JsonPropertyName("volumeReplicationStatus")] VolumeReplicationStatus VolumeReplicationStatus);
}