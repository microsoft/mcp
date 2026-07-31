// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Vault;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault;

[CommandMetadata(
    Id = "da7f163e-471c-4d7d-ae00-d41f5f4b939e",
    Name = "update",
    Title = "Update Backup Vault",
    Description = "Updates vault-level settings including storage redundancy, soft delete, immutability, and managed identity.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class VaultUpdateCommand(ILogger<VaultUpdateCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<VaultUpdateOptions, VaultUpdateCommand.VaultUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VaultUpdateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(VaultUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        bool hasUpdate =
            !string.IsNullOrEmpty(options.Redundancy) ||
            !string.IsNullOrEmpty(options.SoftDelete) ||
            !string.IsNullOrEmpty(options.SoftDeleteRetentionDays) ||
            !string.IsNullOrEmpty(options.ImmutabilityState) ||
            !string.IsNullOrEmpty(options.IdentityType) ||
            !string.IsNullOrEmpty(options.Tags);

        if (!hasUpdate)
        {
            validationResult.Errors.Add(
                "At least one update option must be provided: --redundancy, --soft-delete, --soft-delete-retention-days, --immutability-state, --identity-type, or --tags.");
        }

        if (!string.IsNullOrEmpty(options.SoftDeleteRetentionDays))
        {
            if (!int.TryParse(options.SoftDeleteRetentionDays, out var retentionDays) || retentionDays < 14 || retentionDays > 180)
            {
                validationResult.Errors.Add("--soft-delete-retention-days must be an integer between 14 and 180.");
            }
        }

        if (!string.IsNullOrEmpty(options.IdentityType) &&
            !options.IdentityType.Equals("SystemAssigned", StringComparison.OrdinalIgnoreCase) &&
            !options.IdentityType.Equals("UserAssigned", StringComparison.OrdinalIgnoreCase) &&
            !options.IdentityType.Equals("None", StringComparison.OrdinalIgnoreCase) &&
            !options.IdentityType.Equals("SystemAssigned,UserAssigned", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--identity-type must be 'SystemAssigned', 'UserAssigned', 'SystemAssigned,UserAssigned', or 'None'.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VaultUpdateOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);

        try
        {
            var result = await _azureBackupService.UpdateVaultAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.VaultType,
                options.Redundancy,
                options.SoftDelete,
                options.SoftDeleteRetentionDays,
                options.ImmutabilityState,
                options.IdentityType,
                options.Tags,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.VaultUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vault. Vault: {Vault}, ResourceGroup: {ResourceGroup}",
                options.Vault, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found. Verify the vault name and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Update conflict. The vault settings may have been modified concurrently.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record VaultUpdateCommandResult(OperationResult Result);
}
