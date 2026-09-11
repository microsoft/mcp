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
    Id = "9a44e3ce-cd56-4d03-9ac8-5fc73e7d66d5",
    Name = "authorize-external-replication",
    Description = "Start SVM peering for an external Azure NetApp Files replication and return the acceptance command.",
    Title = "Authorize External NetApp Files Replication",
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationAuthorizeExternalReplicationCommand(ILogger<ReplicationAuthorizeExternalReplicationCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver) 
    : SubscriptionCommand<ReplicationActionOptions, ReplicationAuthorizeExternalReplicationCommand.ReplicationAuthorizeExternalReplicationCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ReplicationAuthorizeExternalReplicationCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ReplicationActionOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ReplicationCommandHelpers.ValidateVolumeTarget(options);
            ReplicationCommandHelpers.ValidateUnsupportedActionOptions(options);
            var result = await _netAppFilesService.AuthorizeExternalReplication(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.SvmPeerCommandInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authorizing external NetApp Files replication. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ReplicationAuthorizeExternalReplicationCommandResult([property: JsonPropertyName("svmPeerCommandInfo")] ReplicationOperationResult SvmPeerCommandInfo);
}