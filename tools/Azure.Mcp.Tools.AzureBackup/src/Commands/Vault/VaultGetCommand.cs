// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Vault;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault;

/// <summary>
/// Consolidated vault command: when --vault is supplied returns a single vault's details;
/// otherwise lists all vaults in the subscription (optionally filtered by --vault-type).
/// </summary>
[CommandMetadata(
    Id = "4a1084d5-50d9-489f-9e4c-acc594441b1f",
    Name = "get",
    Title = "Get Backup Vault",
    Description = """
        Retrieves backup vault information. When --vault and --resource-group are specified,
        returns detailed information about a single vault including type, location, SKU, and
        storage redundancy. When omitted, lists all backup vaults (RSV and Backup vaults) in
        the subscription. Optionally filter by --vault-type ('rsv' or 'dpp') and/or
        --resource-group to narrow the listing results.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class VaultGetCommand(ILogger<VaultGetCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<VaultGetOptions, VaultGetCommand.VaultGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VaultGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(VaultGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrEmpty(options.Vault) && string.IsNullOrEmpty(options.ResourceGroup))
        {
            validationResult.Errors.Add("--resource-group is required when --vault is specified.");
        }

        if (!string.IsNullOrEmpty(options.VaultType) &&
            !options.VaultType.Equals("rsv", StringComparison.OrdinalIgnoreCase) &&
            !options.VaultType.Equals("dpp", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--vault-type must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VaultGetOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.AddTag(AzureBackupTelemetryTags.OperationScope, string.IsNullOrEmpty(options.Vault) ? "list" : "single");

        try
        {
            if (!string.IsNullOrEmpty(options.Vault))
            {
                var vault = await _azureBackupService.GetVaultAsync(
                    options.Vault,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new([vault]),
                    AzureBackupJsonContext.Default.VaultGetCommandResult);
            }
            else
            {
                var vaults = await _azureBackupService.ListVaultsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(vaults),
                    AzureBackupJsonContext.Default.VaultGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vault(s). Vault: {Vault}, ResourceGroup: {ResourceGroup}",
                options.Vault, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Vault not found. Verify the vault name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found. Verify the vault name and resource group.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record VaultGetCommandResult(List<BackupVaultInfo> Vaults);
}
