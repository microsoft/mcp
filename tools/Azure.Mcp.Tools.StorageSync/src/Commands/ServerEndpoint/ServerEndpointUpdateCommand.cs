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

namespace Azure.Mcp.Tools.StorageSync.Commands.ServerEndpoint;

public sealed class ServerEndpointUpdateCommand(ILogger<ServerEndpointUpdateCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<ServerEndpointUpdateOptions>
{
    private const string CommandTitle = "Update Server Endpoint";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<ServerEndpointUpdateCommand> _logger = logger;

    public override string Id => "7b35bb46-0a34-4e44-9d7c-148e9992b445";

    public override string Name => "update";

    public override string Description => "Update properties of a server endpoint.";

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
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.Name.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.CloudTiering.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.VolumeFreeSpacePercent.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.TierFilesOlderThanDays.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.LocalCacheMode.AsOptional());
    }

    protected override ServerEndpointUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.SyncGroupName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.SyncGroup.Name.Name);
        options.ServerEndpointName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.ServerEndpoint.Name.Name);
        options.CloudTiering = parseResult.GetValueOrDefault<bool?>(StorageSyncOptionDefinitions.ServerEndpoint.CloudTiering.Name);
        options.VolumeFreeSpacePercent = parseResult.GetValueOrDefault<int>(StorageSyncOptionDefinitions.ServerEndpoint.VolumeFreeSpacePercent.Name);
        options.TierFilesOlderThanDays = parseResult.GetValueOrDefault<int>(StorageSyncOptionDefinitions.ServerEndpoint.TierFilesOlderThanDays.Name);
        options.LocalCacheMode = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.ServerEndpoint.LocalCacheMode.Name);
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
            _logger.LogInformation("Updating server endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}, CloudTiering: {CloudTiering}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName, options.ServerEndpointName, options.CloudTiering);

            var endpoint = await _service.UpdateServerEndpointAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.ServerEndpointName!,
                options.CloudTiering,
                options.VolumeFreeSpacePercent,
                options.TierFilesOlderThanDays,
                options.LocalCacheMode,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new ServerEndpointUpdateCommandResult(endpoint);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.ServerEndpointUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating server endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(ServerEndpointUpdateCommandResult))]
    internal record ServerEndpointUpdateCommandResult(ServerEndpointDataSchema Result);
}
