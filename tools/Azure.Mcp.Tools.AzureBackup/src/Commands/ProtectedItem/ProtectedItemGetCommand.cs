// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;

/// <summary>
/// Consolidated protected item command: when --protected-item is supplied returns a single
/// item's details; otherwise lists all protected items in the vault.
/// </summary>
[CommandMetadata(
    Id = "bc985e4f-8945-447a-9aba-ef13df309001",
    Name = "get",
    Title = "Get Protected Item",
    Description = """
        Retrieves protected item information. When --protected-item is specified, returns
        detailed information about a single backup instance including protection status,
        datasource details, policy assignment, and last backup time. Specify --container
        for RSV workload items. When --protected-item is omitted, lists all protected items
        (backup instances) in the vault.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ProtectedItemGetCommand(ILogger<ProtectedItemGetCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<BaseProtectedItemOptions, ProtectedItemGetCommand.ProtectedItemGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ProtectedItemGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseProtectedItemOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.AddTag(AzureBackupTelemetryTags.OperationScope, string.IsNullOrEmpty(options.ProtectedItem) ? "list" : "single");

        try
        {
            if (!string.IsNullOrEmpty(options.ProtectedItem))
            {
                var item = await _azureBackupService.GetProtectedItemAsync(
                    options.Vault,
                    options.ResourceGroup,
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
                    options.Vault,
                    options.ResourceGroup,
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
        RequestFailedException reqEx when reqEx.ErrorCode == "BMSUserErrorContainerNameIncorrectFormat" =>
            $"Container name format is incorrect. Use the fully qualified container name from 'azurebackup protecteditem get' (e.g., 'IaasVMContainer;iaasvmcontainerv2;resourceGroup;vmName'). Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.ErrorCode == "BMSUserErrorProtectedItemNameIncorrectFormat" =>
            $"Protected item name format is incorrect. Use the fully qualified name from 'azurebackup protecteditem get' (e.g., 'VM;iaasvmcontainerv2;resourceGroup;vmName'). Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed. Ensure the caller has the Backup Reader or Backup Operator role on the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ProtectedItemGetCommandResult(List<ProtectedItemInfo> ProtectedItems);
}
