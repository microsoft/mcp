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
    Id = "bd4d3862-d3fa-47b9-bd39-8ffd31f9cda2",
    Name = "populate-availability-zone",
    Description = "Populate availability zone information for an Azure NetApp Files volume.",
    Title = "Populate NetApp Files Volume Availability Zone",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    LocalRequired = false,
    Secret = false
)]
public sealed class ReplicationPopulateAvailabilityZoneCommand(ILogger<ReplicationPopulateAvailabilityZoneCommand> logger, INetAppFilesService netAppFilesService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ReplicationActionOptions, ReplicationPopulateAvailabilityZoneCommand.ReplicationPopulateAvailabilityZoneCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ReplicationPopulateAvailabilityZoneCommand> _logger = logger;
    private readonly INetAppFilesService _netAppFilesService = netAppFilesService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ReplicationActionOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ReplicationCommandHelpers.ValidateVolumeTarget(options);
            ReplicationCommandHelpers.ValidateUnsupportedActionOptions(options);
            var result = await _netAppFilesService.PopulateAvailabilityZone(options.Account, options.Pool, options.Volume, options.ResourceGroup, options.Ids, options.Subscription!, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, NetAppFilesJsonContext.Default.NetAppVolumeCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating NetApp Files availability zone. Volume: {Volume}, Subscription: {Subscription}", options.Volume, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ReplicationPopulateAvailabilityZoneCommandResult([property: JsonPropertyName("netAppVolumeCreateResult")] NetAppVolumeCreateResult NetAppVolumeCreateResult);
}