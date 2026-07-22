// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options.StorageSyncService;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;

[CommandMetadata(
    Id = "77734a55-8290-4c16-8b37-cf37277f018f",
    Name = "get",
    Title = "Get Storage Sync Service",
    Description = "Retrieve Azure Storage Sync service details or list all Storage Sync services. Use --name to get a specific service, or omit it to list all services in the subscription or resource group. Shows service properties, location, provisioning state, and configuration.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class StorageSyncServiceGetCommand(ILogger<StorageSyncServiceGetCommand> logger, IStorageSyncService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<StorageSyncServiceGetOptions, StorageSyncServiceGetCommand.StorageSyncServiceGetCommandResult>(subscriptionResolver)
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<StorageSyncServiceGetCommand> _logger = logger;

    public override void ValidateOptions(StorageSyncServiceGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrEmpty(options.ResourceGroup) && !string.IsNullOrEmpty(options.Name))
        {
            validationResult.Errors.Add("Missing Required options: --resource-group is required when --name is specified");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, StorageSyncServiceGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // If name is provided, get specific service
            if (!string.IsNullOrEmpty(options.Name))
            {
                _logger.LogInformation("Getting storage sync service. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                    options.Subscription, options.ResourceGroup, options.Name);

                var service = await _service.GetStorageSyncServiceAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.Name,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (service == null)
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = "Storage sync service not found";
                    return context.Response;
                }

                context.Response.Results = ResponseResult.Create(new([service]), StorageSyncJsonContext.Default.StorageSyncServiceGetCommandResult);
            }
            else
            {
                // List all services
                _logger.LogInformation("Listing storage sync services. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}",
                    options.Subscription, options.ResourceGroup);

                var services = await _service.ListStorageSyncServicesAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(services ?? []), StorageSyncJsonContext.Default.StorageSyncServiceGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage sync service(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record StorageSyncServiceGetCommandResult(List<StorageSyncServiceDataSchema> Results);
}
