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

public sealed class RegisteredServerUpdateCommand(ILogger<RegisteredServerUpdateCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<RegisteredServerUpdateOptions>
{
    private const string CommandTitle = "Update Registered Server";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<RegisteredServerUpdateCommand> _logger = logger;

    public override string Id => "c443ed00-f17f-46a8-a5d3-df128aa1606b";

    public override string Name => "update";

    public override string Description => "Update properties of a registered server.";

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

    protected override RegisteredServerUpdateOptions BindOptions(ParseResult parseResult)
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
            _logger.LogInformation("Updating registered server. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}, ServerId: {ServerId}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName, options.RegisteredServerId);

            var server = await _service.UpdateServerAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.RegisteredServerId!,
                null,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new RegisteredServerUpdateCommandResult(server);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.RegisteredServerUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registered server");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(RegisteredServerUpdateCommandResult))]
    internal record RegisteredServerUpdateCommandResult(RegisteredServerDataSchema Result);
}
