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

public sealed class RegisteredServerListCommand(ILogger<RegisteredServerListCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<RegisteredServerListOptions>
{
    private const string CommandTitle = "List Registered Servers";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<RegisteredServerListCommand> _logger = logger;

    public override string Id => "e1h7i9g3-6f8j-8h2i-3g7j-0i3h6j9k3l4m";

    public override string Name => "list";

    public override string Description => "List all registered servers in a Storage Sync service.";

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
    }

    protected override RegisteredServerListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);
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
            _logger.LogInformation("Listing registered servers. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ServiceName: {ServiceName}",
                options.Subscription, options.ResourceGroup, options.StorageSyncServiceName);

            var servers = await _service.ListRegisteredServersAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.StorageSyncServiceName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new RegisteredServerListCommandResult(servers ?? []);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.RegisteredServerListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing registered servers");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(RegisteredServerListCommandResult))]
    internal record RegisteredServerListCommandResult(List<RegisteredServerData> Results);
}
