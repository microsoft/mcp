// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;

[CommandMetadata(
    Id = "b7e14d02-7c9f-4d3b-9e9c-8c8f2b0a1e34",
    Name = "update-protection",
    Title = "Update VM Protection",
    Description = """
        Updates the backup configuration of an already-protected Azure IaaS VM. Supports
        changing the attached backup policy and/or the selective disk backup configuration
        (--disk-list-setting, --disks-list, --exclude-all-data-disks). Only supported for
        RSV (Recovery Services vault) IaaS VM protected items. Pass the VM ARM resource ID
        as --datasource-id. At least one of --policy, --disk-list-setting, --disks-list,
        or --exclude-all-data-disks must be provided. The operation is asynchronous; use
        'azurebackup job get' to monitor the resulting ConfigureBackup job.
        See https://learn.microsoft.com/azure/backup/selective-disk-backup-restore for
        selective disk semantics.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ProtectedItemUpdateProtectionCommand(ILogger<ProtectedItemUpdateProtectionCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<ProtectedItemUpdateProtectionOptions, ProtectedItemUpdateProtectionCommand.ProtectedItemUpdateProtectionCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ProtectedItemUpdateProtectionCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(ProtectedItemUpdateProtectionOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        DiskExclusionValidator.ValidateDiskExclusionOptions(
            options.DiskListSetting,
            options.DisksList,
            options.ExcludeAllDataDisks,
            validationResult);

        // Command must change something to be useful.
        var hasPolicy = !string.IsNullOrWhiteSpace(options.Policy);
        var hasDiskChange =
            !string.IsNullOrWhiteSpace(options.DiskListSetting) ||
            !string.IsNullOrWhiteSpace(options.DisksList) ||
            options.ExcludeAllDataDisks;

        if (!hasPolicy && !hasDiskChange)
        {
            validationResult.Errors.Add(
                "At least one of --policy, --disk-list-setting, --disks-list, or --exclude-all-data-disks must be provided.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ProtectedItemUpdateProtectionOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);

        try
        {
            var diskExclusion = DiskExclusionValidator.BuildDiskExclusionSpec(
                options.DiskListSetting,
                options.DisksList,
                options.ExcludeAllDataDisks);

            var result = await _azureBackupService.UpdateProtectionAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.DatasourceId,
                options.Policy,
                diskExclusion,
                options.VaultType,
                options.Container,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.ProtectedItemUpdateProtectionCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating VM protection. DatasourceId: {DatasourceId}, Vault: {Vault}",
                options.DatasourceId, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        NotSupportedException notSupEx => notSupEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the protected item. Ensure the caller has Backup Contributor role. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "The specified VM is not currently protected in this vault. Use 'azurebackup protecteditem protect' to configure protection first.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        NotSupportedException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    public sealed record ProtectedItemUpdateProtectionCommandResult(ProtectResult Result);
}
