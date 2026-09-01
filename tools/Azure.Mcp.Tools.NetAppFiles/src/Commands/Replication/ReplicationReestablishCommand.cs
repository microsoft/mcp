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
    Id = "f9c4a7ee-c3bb-4a7f-9a26-ae39f0aa7e45",
    Name = "reestablish",
    Description = "Re-establish Azure NetApp Files replication between previously paired volumes.",
    Title = "Re-Establish NetApp Files Replication",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationReestablishCommand(ILogger<ReplicationReestablishCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ReplicationReestablishOptions, ReplicationReestablishCommand.ReplicationReestablishCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ReplicationReestablishCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ReplicationReestablishOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ReplicationCommandHelpers.ValidateVolumeTarget(options);
            ReplicationCommandHelpers.ValidateUnsupportedActionOptions(options);
            if (string.IsNullOrWhiteSpace(options.SourceVolumeId))
            {
                throw new ArgumentException("Provide --sourceVolumeId to re-establish a replication.");
            }

            var result = await _netAppFilesService.ReestablishReplication(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.SourceVolumeId, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ReplicationOperationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error re-establishing NetApp Files replication. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ReplicationReestablishCommandResult([property: JsonPropertyName("replicationReestablishResult")] ReplicationOperationResult ReplicationOperationResult);
}