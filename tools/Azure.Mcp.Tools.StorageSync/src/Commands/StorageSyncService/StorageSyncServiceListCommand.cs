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

namespace Azure.Mcp.Tools.StorageSync.Commands.StorageSyncService;

public sealed class StorageSyncServiceListCommand(ILogger<StorageSyncServiceListCommand> logger, IStorageSyncService service) : BaseStorageSyncCommand<StorageSyncServiceListOptions>
{
    private const string CommandTitle = "List Storage Sync Services";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<StorageSyncServiceListCommand> _logger = logger;

    public override string Id => "c6d3f5e9-2b4g-5c8f-ae3d-6g9c2e4f7h8d";

    public override string Name => "list";

    public override string Description => "List all Azure Storage Sync services in a subscription or resource group.";

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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
    }

    protected override StorageSyncServiceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            _logger.LogInformation("Listing storage sync services. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}",
                options.Subscription, options.ResourceGroup);

            var services = await _service.ListStorageSyncServicesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var results = new StorageSyncServiceListCommandResult(services ?? []);
            context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.StorageSyncServiceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing storage sync services");
            HandleException(context, ex);
        }

        return context.Response;
    }

    [JsonSerializable(typeof(StorageSyncServiceListCommandResult))]
    internal record StorageSyncServiceListCommandResult(List<StorageSyncServiceDataSchema> Results);
}

/// <summary>
/// Options for listing storage sync services.
/// </summary>
public class StorageSyncServiceListOptions : BaseStorageSyncOptions
{
}
