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

    public override string Id => "f2i8j0h4-7g9k-9i3j-4h8k-1j4i7k0l4m5n";

    public override string Name => "get";

    public override string Description => "Get details about a specific registered server.";

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
        command.Options.Add(StorageSyncOptionDefinitions.RegisteredServer.ServerId.AsRequired());
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

            var results = new RegisteredServerGetCommandResult(server);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.RegisteredServerGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting registered server");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(RegisteredServerGetCommandResult))]
    internal record RegisteredServerGetCommandResult(RegisteredServerData Result);
}
