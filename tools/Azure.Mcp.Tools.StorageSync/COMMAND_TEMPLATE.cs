// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.StorageSync.Options;
using Azure.Mcp.Tools.StorageSync.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.StorageSync.Commands.{RESOURCE};

/// <summary>
/// {COMMAND_DESCRIPTION}
/// </summary>
public sealed class {COMMAND_NAME}(ILogger<{COMMAND_NAME}> logger, IStorageSyncService service)
    : BaseStorageSyncCommand<{OPTIONS_NAME}>
{
    private const string CommandTitle = "{HUMAN_READABLE_TITLE}";
    private readonly IStorageSyncService _service = service;
    private readonly ILogger<{COMMAND_NAME}> _logger = logger;

    public override string Name => "{command_group_name}";
    public override string Description => "{COMMAND_DESCRIPTION}";

    public override ToolMetadata ToolMetadata => new()
    {
        OpenWorld = false,
        Destructive = {IS_DESTRUCTIVE},      // true for Delete operations, false otherwise
        Idempotent = {IS_IDEMPOTENT},        // true for Get/List/Set-to-value, false for Create/Generate
        ReadOnly = {IS_READ_ONLY},           // true for Get/List, false for Create/Update/Delete
        Secret = false,                       // true if returns credentials/keys, false otherwise
        LocalRequired = false                 // true if requires local tools, false for cloud API only
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        // Register required options
        command.Options.Add(OptionDefinitions.Common.Subscription.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(StorageSyncOptionDefinitions.StorageSyncService.Name.AsRequired());

        // Register optional options as needed
        // command.Options.Add(StorageSyncOptionDefinitions.SyncGroup.Name.AsOptional());
    }

    protected override {OPTIONS_NAME} BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        // Bind command-specific options
        options.StorageSyncServiceName = parseResult.GetValueOrDefault<string>(StorageSyncOptionDefinitions.StorageSyncService.Name.Name);

        return options;
    }

    protected override async Task ExecuteAsync(CommandContext context, {OPTIONS_NAME} options)
    {
        try
        {
            _logger.LogInformation("{CommandTitle}. Options: {@Options}", CommandTitle, options);

            // TODO: Implement service call
            // var result = await _service.{METHOD_NAME}Async(
            //     options.Subscription!,
            //     options.ResourceGroup!,
            //     options.StorageSyncServiceName!,
            //     options.Tenant,
            //     options.RetryPolicy,
            //     context.CancellationToken);

            // Create result record
            // var results = new {COMMAND_NAME}.{COMMAND_NAME}CommandResult(result ?? []);
            // context.Response.Results = ResponseResult.Create(results, StorageSyncJsonContext.Default.{COMMAND_NAME}CommandResult);

            // TODO: Remove placeholder
            context.Response.Results = ResponseResult.Create(new { Message = "Not implemented" }, StorageSyncJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}", Name);
            HandleException(context, ex);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            $"Resource not found. Verify the resource exists and you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Resource already exists or is in use.",
        Azure.Identity.AuthenticationFailedException =>
            "Authentication failed. Please run 'Connect-AzAccount' to sign in.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        Azure.Identity.AuthenticationFailedException => HttpStatusCode.Unauthorized,
        _ => base.GetStatusCode(ex)
    };

    internal record {COMMAND_NAME}CommandResult(object Result);
}

/// <summary>
/// Options for {COMMAND_NAME}.
/// </summary>
public class {OPTIONS_NAME} : BaseStorageSyncOptions
{
    // TODO: Add command-specific options as properties
    // public string? SyncGroupName { get; set; }
    // public string? CloudEndpointName { get; set; }
}
