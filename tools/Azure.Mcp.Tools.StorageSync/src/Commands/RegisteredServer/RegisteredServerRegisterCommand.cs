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

namespace Azure.Mcp.Tools.StorageSync.Commands.RegisteredServer;

public sealed class RegisteredServerRegisterCommand(ILogger<RegisteredServerRegisterCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<RegisteredServerRegisterOptions>
{
    private const string CommandTitle = "Register Server";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<RegisteredServerRegisterCommand> _logger = logger;

    public override string Id => "g3j9k1i5-8h0l-0j4k-5i9l-2k5j8l1m5n6o";

    public override string Name => "register";

    public override string Description => "Register a new server with a Storage Sync service.";

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
        command.Options.Add(StorageSyncOptionDefinitions.RegisteredServer.ServerId.AsRequired());
    }

    protected override RegisteredServerRegisterOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
        options.RegisteredServerId = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.RegisteredServer.ServerId.Name);
        return options;
    }

    /// <summary>
    /// TODO : Remove this command as its a hybrid command and not needed in MCP tools
    /// </summary>
    /// <param name="context"></param>
    /// <param name="parseResult"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {

        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            _logger.LogInformation("Registering server. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, ServerId: {ServerId}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.RegisteredServerId);

            var server = await _service.RegisterServerAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.RegisteredServerId!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new RegisteredServerRegisterCommandResult(server);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.RegisteredServerRegisterCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering server");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(RegisteredServerRegisterCommandResult))]
    internal record RegisteredServerRegisterCommandResult(RegisteredServerData Result);
}
