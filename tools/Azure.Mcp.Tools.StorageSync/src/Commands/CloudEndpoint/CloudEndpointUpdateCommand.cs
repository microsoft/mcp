// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options.CloudEndpoint;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;

[CommandMetadata(
    Id = "5e76c81b-7964-4e66-b7af-2f4be45144bf",
    Name = "update",
    Title = "Update Cloud Endpoint",
    Description = "Update a Storage Sync cloud endpoint's change enumeration interval. The interval must be between 1 and 20 days. This is a potentially long-running operation.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class CloudEndpointUpdateCommand(
    ILogger<CloudEndpointUpdateCommand> logger,
    IStorageSyncService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<CloudEndpointUpdateOptions, CloudEndpointUpdateCommand.CloudEndpointUpdateCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointUpdateCommand> _logger = logger;

    public override void ValidateOptions(CloudEndpointUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!options.ChangeEnumerationIntervalDays.HasValue)
        {
            validationResult.Errors.Add("At least one update property must be provided (change-enumeration-interval-days).");
        }
        else if (options.ChangeEnumerationIntervalDays is < 1 or > 20)
        {
            validationResult.Errors.Add("Change enumeration interval days must be between 1 and 20.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        CloudEndpointUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Updating cloud endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}, ChangeEnumerationIntervalDays: {ChangeEnumerationIntervalDays}",
                options.Subscription,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.CloudEndpointName,
                options.ChangeEnumerationIntervalDays);

            var endpoint = await _service.UpdateCloudEndpointAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Name,
                options.SyncGroupName,
                options.CloudEndpointName,
                options.ChangeEnumerationIntervalDays!.Value,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(endpoint),
                StorageSyncJsonContext.Default.CloudEndpointUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cloud endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record CloudEndpointUpdateCommandResult(CloudEndpointDataSchema Result);
}
