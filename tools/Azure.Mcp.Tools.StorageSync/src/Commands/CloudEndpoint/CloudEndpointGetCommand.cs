// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;

public sealed class CloudEndpointGetCommand(ILogger<CloudEndpointGetCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<CloudEndpointGetOptions>
{
    private const string CommandTitle = "Get Cloud Endpoint";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointGetCommand> _logger = logger;

    public override string Id => "25dd8bb3-5ba3-4c0d-993d-54917f63d52e";

    public override string Name => "get";

    public override string Description => "List all cloud endpoints in a sync group or retrieve details about a specific cloud endpoint. Returns cloud endpoint properties including Azure File Share configuration, storage account details, and provisioning state. Use --cloud-endpoint-name for a specific endpoint.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Name.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.SyncGroup.Name.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.Name.AsOptional());
    }

    protected override CloudEndpointGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.SyncGroupName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.SyncGroup.Name.Name);
        options.CloudEndpointName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.CloudEndpoint.Name.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // If cloud endpoint name is provided, get specific endpoint
            if (!string.IsNullOrEmpty(options.CloudEndpointName))
            {
                _logger.LogInformation("Getting cloud endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                    options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName, options.CloudEndpointName);

                var endpoint = await _service.GetCloudEndpointAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.StorageSyncServiceName!,
                    options.SyncGroupName!,
                    options.CloudEndpointName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (endpoint == null)
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = "Cloud endpoint not found";
                    return context.Response;
                }

                var singleResult = new CloudEndpointGetCommandResult([endpoint]);
                context.Response.Results = ResponseResult.Create(singleResult, StorageSyncJsonContext.Default.CloudEndpointGetCommandResult);
            }
            else
            {
                // List all cloud endpoints
                _logger.LogInformation("Listing cloud endpoints. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                    options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName);

                var endpoints = await _service.ListCloudEndpointsAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.StorageSyncServiceName!,
                    options.SyncGroupName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var results = new CloudEndpointGetCommandResult(endpoints ?? []);
                context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.CloudEndpointGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cloud endpoint(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(CloudEndpointGetCommandResult))]
    internal record CloudEndpointGetCommandResult(List<CloudEndpointDataSchema> Results);
}
