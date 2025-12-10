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

    public override string Id => "n0q6r8p2-5o7s-7q1r-2p6s-9r2q5s8t2u3v";

    public override string Name => "get";

    public override string Description => "Get details about a specific cloud endpoint.";

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
        command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.Name.AsRequired());
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

            var results = new CloudEndpointGetCommandResult(endpoint);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.CloudEndpointGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cloud endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(CloudEndpointGetCommandResult))]
    internal record CloudEndpointGetCommandResult(CloudEndpointData Result);
}
