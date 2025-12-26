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

namespace Azure.Mcp.Tools.StorageSync.Commands.ServerEndpoint;

public sealed class ServerEndpointDeleteCommand(ILogger<ServerEndpointDeleteCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<ServerEndpointDeleteOptions>
{
    private const string CommandTitle = "Delete Server Endpoint";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<ServerEndpointDeleteCommand> _logger = logger;

    public override string Id => "ef6c2aa9-bb64-4f94-b18b-018e04b504c9";

    public override string Name => "delete";

    public override string Description => "Delete a server endpoint from a sync group.";

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
    }

    protected override ServerEndpointDeleteOptions BindOptions(ParseResult parseResult)
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
            _logger.LogInformation("Deleting server endpoint. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, GroupName: {GroupName}, EndpointName: {EndpointName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.SyncGroupName, options.ServerEndpointName);

            await _service.DeleteServerEndpointAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.SyncGroupName!,
                options.ServerEndpointName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Server endpoint deleted successfully";
            var results = new ServerEndpointDeleteCommandResult("Server endpoint deleted successfully");
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.ServerEndpointDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting server endpoint");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(ServerEndpointDeleteCommandResult))]
    internal record ServerEndpointDeleteCommandResult(string Message);
}
