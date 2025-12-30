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

namespace Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;

public sealed class RegisteredServerUnregisterCommand(ILogger<RegisteredServerUnregisterCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<RegisteredServerUnregisterOptions>
{
    private const string CommandTitle = "Unregister Server";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<RegisteredServerUnregisterCommand> _logger = logger;

    public override string Id => "346661e1-64be-463a-96c6-3626966f55fa";

    public override string Name => "unregister";

    public override string Description => "Unregister a server from a Storage Sync service.";

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
        command.Options.Add(StorageSyncOptionDefinitions.RegisteredServer.ServerId.AsRequired());
    }

    protected override RegisteredServerUnregisterOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.RegisteredServerId = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.RegisteredServer.ServerId.Name);
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
            _logger.LogInformation("Unregistering server. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, ServerId: {ServerId}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.RegisteredServerId);

            await _service.UnregisterServerAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.RegisteredServerId!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Message = "Server unregistered successfully";
            var results = new RegisteredServerUnregisterCommandResult("Server unregistered successfully");
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.RegisteredServerUnregisterCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering server");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(RegisteredServerUnregisterCommandResult))]
    internal record RegisteredServerUnregisterCommandResult(string Message);
}
