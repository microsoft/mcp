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

public sealed class ServerEndpointListCommand(ILogger<ServerEndpointListCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<ServerEndpointListOptions>
{
    private const string CommandTitle = "List Server Endpoints";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<ServerEndpointListCommand> _logger = logger;

    public override string Id => "r4u0v2t6-9s1w-1u5v-6t0w-3v6u9w2x6y7z";

    public override string Name => "list";

    public override string Description => "List all server endpoints in a sync group.";

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

    protected override ServerEndpointListOptions BindOptions(ParseResult parseResult)
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
            _logger.LogInformation("Listing server endpoints. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName);

            var endpoints = await _service.ListServerEndpointsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new ServerEndpointListCommandResult(endpoints ?? []);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.ServerEndpointListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing server endpoints");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(ServerEndpointListCommandResult))]
    internal record ServerEndpointListCommandResult(List<ServerEndpointDataSchema> Results);
}
