// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

public sealed class CloudEndpointListCommand(ILogger<CloudEndpointListCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<CloudEndpointListOptions>
{
    private const string CommandTitle = "List Cloud Endpoints";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointListCommand> _logger = logger;

    public override string Id => "m9p5q7o1-4n6r-6p0q-1o5r-8q1p4r7s1t2u";

    public override string Name => "list";

    public override string Description => "List all cloud endpoints in a sync group.";

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
    }

    protected override CloudEndpointListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.SyncGroupName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.SyncGroup.Name.Name);
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

            var results = new CloudEndpointListCommandResult(endpoints ?? []);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.CloudEndpointListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing cloud endpoints");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(CloudEndpointListCommandResult))]
    internal record CloudEndpointListCommandResult(List<CloudEndpointData> Results);
}
