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

namespace Azure.Mcp.Tools.StorageSync.Commands.ServerEndpoint;

public sealed class ServerEndpointGetCommand(ILogger<ServerEndpointGetCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<ServerEndpointGetOptions>
{
    private const string CommandTitle = "Get Server Endpoint";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<ServerEndpointGetCommand> _logger = logger;

    public override string Id => "s5v1w3u7-0t2x-2v6w-7u1x-4w7v0x3y7z8a";

    public override string Name => "get";

    public override string Description => "Get details about a specific server endpoint.";

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
        command.Options.Add(StorageSyncOptionDefinitions.ServerEndpoint.Name.AsRequired());
    }

    protected override ServerEndpointGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.SyncGroupName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.SyncGroup.Name.Name);
        options.ServerEndpointName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.ServerEndpoint.Name.Name);
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
            _logger.LogInformation("Getting server endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName, options.ServerEndpointName);

            var endpoint = await _service.GetServerEndpointAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.ServerEndpointName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            if (endpoint == null)
            {
                context.Response.Status = HttpStatusCode.NotFound;
                context.Response.Message = "Server endpoint not found";
                return context.Response;
            }

            var results = new ServerEndpointGetCommandResult(endpoint);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.ServerEndpointGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(ServerEndpointGetCommandResult))]
    internal record ServerEndpointGetCommandResult(ServerEndpointDataSchema Result);
}
