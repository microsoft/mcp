// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.StorageSync.Options;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.StorageSync.Commands.CloudEndpoint;

public sealed class CloudEndpointTriggerChangeDetectionCommand(ILogger<CloudEndpointTriggerChangeDetectionCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<CloudEndpointTriggerChangeDetectionOptions>
{
    private const string CommandTitle = "Trigger Change Detection";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<CloudEndpointTriggerChangeDetectionCommand> _logger = logger;

    public override string Id => "96f096a2-d36f-4361-aa74-4e393e7f48a5";

    public override string Name => "changedetection";

    public override string Description => "Trigger change detection on a cloud endpoint to sync file changes.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
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
        command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.DirectoryPath.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.ChangeDetectionMode.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.CloudEndpoint.Paths.AsOptional());
    }

    protected override CloudEndpointTriggerChangeDetectionOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.SyncGroupName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.SyncGroup.Name.Name);
        options.CloudEndpointName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.CloudEndpoint.Name.Name);
        options.DirectoryPath = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.CloudEndpoint.DirectoryPath.Name);
        options.ChangeDetectionMode = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.CloudEndpoint.ChangeDetectionMode.Name);
        options.Paths = parseResult.GetValueOrDefault<string[]>(StorageSyncOptionDefinitions.CloudEndpoint.Paths.Name)?.ToList();
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
            _logger.LogInformation("Triggering change detection. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}, DirectoryPath: {DirectoryPath}, ChangeDetectionMode: {ChangeDetectionMode}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName, options.CloudEndpointName, options.DirectoryPath, options.ChangeDetectionMode);

            await _service.TriggerChangeDetectionAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.CloudEndpointName!,
                options.DirectoryPath!,
                options.ChangeDetectionMode,
                options.Paths,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Change detection triggered successfully";
            var results = new CloudEndpointTriggerChangeDetectionCommandResult("Change detection triggered successfully");
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.CloudEndpointTriggerChangeDetectionCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering change detection");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(CloudEndpointTriggerChangeDetectionCommandResult))]
    internal record CloudEndpointTriggerChangeDetectionCommandResult(string Message);
}
