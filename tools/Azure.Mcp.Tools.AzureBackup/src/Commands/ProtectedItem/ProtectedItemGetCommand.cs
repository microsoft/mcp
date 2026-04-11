// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;

/// <summary>
/// Consolidated protected item command: when --protected-item is supplied returns a single
/// item's details; otherwise lists all protected items in the vault.
/// </summary>
public sealed class ProtectedItemGetCommand(ILogger<ProtectedItemGetCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<BaseProtectedItemOptions>()
{
    private const string CommandTitle = "Get Protected Item";
    private readonly ILogger<ProtectedItemGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "bc985e4f-8945-447a-9aba-ef13df309001";
    public override string Name => "get";
    public override string Description =>
        """
        Retrieves protected item information. When --protected-item is specified, returns
        detailed information about a single backup instance including protection status,
        datasource details, policy assignment, and last backup time. Specify --container
        for RSV workload items. When --protected-item is omitted, lists all protected items
        (backup instances) in the vault.
        """;
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
        command.Options.Add(AzureBackupOptionDefinitions.ProtectedItem.AsOptional());
        command.Options.Add(AzureBackupOptionDefinitions.Container);
    }

    protected override BaseProtectedItemOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ProtectedItem = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ProtectedItem.Name);
        options.Container = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Container.Name);
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
            if (!string.IsNullOrEmpty(options.ProtectedItem))
            {
                var item = await _azureBackupService.GetProtectedItemAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.ProtectedItem,
                    options.VaultType,
                    options.Container,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new([item]),
                    AzureBackupJsonContext.Default.ProtectedItemGetCommandResult);
            }
            else
            {
                var items = await _azureBackupService.ListProtectedItemsAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(items),
                    AzureBackupJsonContext.Default.ProtectedItemGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting protected item(s). ProtectedItem: {ProtectedItem}, Vault: {Vault}",
                options.ProtectedItem, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Protected item not found. Verify the item name and vault.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Protected item not found. Verify the item name and vault.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record ProtectedItemGetCommandResult(List<ProtectedItemInfo> ProtectedItems);
}
