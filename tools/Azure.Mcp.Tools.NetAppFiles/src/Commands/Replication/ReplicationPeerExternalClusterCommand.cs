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
    Id = "91c8c4e9-8838-4980-b2dc-a8375c0f6c90",
    Name = "peer-external-cluster",
    Description = "Start peering the external cluster for an Azure NetApp Files migration volume.",
    Title = "Peer External Cluster For NetApp Files Replication",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationPeerExternalClusterCommand(ILogger<ReplicationPeerExternalClusterCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ReplicationPeerExternalClusterOptions, ReplicationPeerExternalClusterCommand.ReplicationPeerExternalClusterCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ReplicationPeerExternalClusterCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ReplicationPeerExternalClusterOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ReplicationCommandHelpers.ValidateVolumeTarget(options);
            ReplicationCommandHelpers.ValidateUnsupportedActionOptions(options);
            if (options.PeerIpAddresses is not { Length: > 0 })
            {
                throw new ArgumentException("Provide at least one --peerIpAddresses value.");
            }

            var result = await _netAppFilesService.PeerExternalCluster(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.PeerIpAddresses, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.ClusterPeerCommandInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error peering external cluster for NetApp Files replication. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ReplicationPeerExternalClusterCommandResult([property: JsonPropertyName("clusterPeerCommandInfo")] ClusterPeerCommandInfo ClusterPeerCommandInfo);
}