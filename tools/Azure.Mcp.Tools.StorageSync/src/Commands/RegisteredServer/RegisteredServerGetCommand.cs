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

namespace Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;

public sealed class RegisteredServerGetCommand(ILogger<RegisteredServerGetCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<RegisteredServerGetOptions>
{
    private const string CommandTitle = "Get Registered Server";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<RegisteredServerGetCommand> _logger = logger;

    public override string Id => "fe3b07c3-9a11-465e-bfb6-6461b85b2e52";

    public override string Name => "get";

    public override string Description => "List all registered servers in a Storage Sync service or retrieve details about a specific registered server. Returns server properties including server ID, registration status, agent version, OS version, and last heartbeat. Use --server-id for a specific server.";

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
        command.Options.Add(StorageSyncOptionDefinitions.RegisteredServer.ServerId.AsOptional());
    }

    protected override RegisteredServerGetOptions BindOptions(ParseResult parseResult)
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
            // If server ID is provided, get specific server
            if (!string.IsNullOrEmpty(options.RegisteredServerId))
            {
                _logger.LogInformation("Getting registered server. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, ServerId: {ServerId}",
                    options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.RegisteredServerId);

                var server = await _service.GetRegisteredServerAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.StorageSyncServiceName!,
                    options.RegisteredServerId!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                if (server == null)
                {
                    context.Response.Status = HttpStatusCode.NotFound;
                    context.Response.Message = "Registered server not found";
                    return context.Response;
                }

                var singleResult = new RegisteredServerGetCommandResult([server]);
                context.Response.Results = ResponseResult.Create(singleResult, StorageSyncJsonContext.Default.RegisteredServerGetCommandResult);
            }
            else
            {
                // List all registered servers
                _logger.LogInformation("Listing registered servers. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                    options.Subscription, options.ResourceGroup, options.StorageSyncServiceName);

                var servers = await _service.ListRegisteredServersAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.StorageSyncServiceName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var results = new RegisteredServerGetCommandResult(servers ?? []);
                context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.RegisteredServerGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registered server(s)");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(RegisteredServerGetCommandResult))]
    internal record RegisteredServerGetCommandResult(List<RegisteredServerDataSchema> Results);
}
