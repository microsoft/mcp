// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.Mcp.Tools.StorageSync.Options;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;

[CommandMetadata(
    Id = "fe3b07c3-9a11-465e-bfb6-6461b85b2e52",
    Name = "get",
    Title = "Get Registered Server",
    Description = "List all registered servers in a Storage Sync service or retrieve details about a specific registered server. Returns server properties including server ID, registration status, agent version, OS version, and last heartbeat. Use --server-id for a specific server.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class RegisteredServerGetCommand(ILogger<RegisteredServerGetCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<RegisteredServerGetOptions>
{
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<RegisteredServerGetCommand> _logger = logger;

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

                context.Response.Results = ResponseResult.Create(new([server]), StorageSyncJsonContext.Default.RegisteredServerGetCommandResult);
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

                context.Response.Results = ResponseResult.Create(new(servers ?? []), StorageSyncJsonContext.Default.RegisteredServerGetCommandResult);
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
