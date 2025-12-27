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

public sealed class ServerEndpointCreateCommand(ILogger<ServerEndpointCreateCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<ServerEndpointCreateOptions>
{
    private const string CommandTitle = "Create Server Endpoint";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<ServerEndpointCreateCommand> _logger = logger;

    public override string Id => "fcbdf461-6fde-4cfb-a944-4a56a2be90e4";

    public override string Name => "create";

    public override string Description => "Add a server endpoint to a sync group by specifying a local server path to sync. Server endpoints represent the on-premises side of the sync relationship and include cloud tiering configuration.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
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
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.ServerResourceId.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.ServerLocalPath.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.CloudTiering.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.VolumeFreeSpacePercent.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.TierFilesOlderThanDays.AsOptional());
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.LocalCacheMode.AsOptional());
    }

    protected override ServerEndpointCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.SyncGroupName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.SyncGroup.Name.Name);
        options.ServerEndpointName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.ServerEndpoint.Name.Name);
        options.ServerResourceId = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.ServerEndpoint.ServerResourceId.Name);
        options.ServerLocalPath = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.ServerEndpoint.ServerLocalPath.Name);
        options.CloudTiering = parseResult.GetValueOrDefault<bool?>(StorageSyncOptionDefinitions.ServerEndpoint.CloudTiering.Name);
        options.VolumeFreeSpacePercent = parseResult.GetValueOrDefault<int?>(StorageSyncOptionDefinitions.ServerEndpoint.VolumeFreeSpacePercent.Name);
        options.TierFilesOlderThanDays = parseResult.GetValueOrDefault<int?>(StorageSyncOptionDefinitions.ServerEndpoint.TierFilesOlderThanDays.Name);
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
            _logger.LogInformation("Creating server endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName, options.ServerEndpointName);

            var endpoint = await _service.CreateServerEndpointAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.ServerEndpointName!,
                options.ServerResourceId!,
                options.ServerLocalPath!,
                options.CloudTiering,
                options.VolumeFreeSpacePercent,
                options.TierFilesOlderThanDays,
                options.LocalCacheMode,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new ServerEndpointCreateCommandResult(endpoint);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.ServerEndpointCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating server endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(ServerEndpointCreateCommandResult))]
    internal record ServerEndpointCreateCommandResult(ServerEndpointDataSchema Result);
}
