// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.BackupPolicy;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.BackupPolicy;

[CommandMetadata(
    Id = "b296a755-0790-4f62-aa08-139d0a302289",
    Name = "update",
    Title = "Update Azure NetApp Files Backup Policy",
    Description = "Updates the retention counts or enabled state of an Azure NetApp Files backup policy. Requires the account, backup policy, at least one property to update, resource group, and subscription. Returns the updated backup policy details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupPolicyUpdateCommand(
    ILogger<BackupPolicyUpdateCommand> logger,
    INetAppFilesBackupPolicyService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupPolicyUpdateOptions, BackupPolicyUpdateCommand.BackupPolicyUpdateResult>(subscriptionResolver)
{
    private readonly ILogger<BackupPolicyUpdateCommand> _logger = logger;
    private readonly INetAppFilesBackupPolicyService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        BackupPolicyUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var backupPolicy = await _service.UpdateBackupPolicyAsync(
                options.Account,
                options.BackupPolicy,
                options.DailyBackupsToKeep,
                options.WeeklyBackupsToKeep,
                options.MonthlyBackupsToKeep,
                options.Enabled,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupPolicyUpdateResult(backupPolicy),
                NetAppFilesJsonContext.Default.BackupPolicyUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating Azure NetApp Files backup policy. Account: {Account}, BackupPolicy: {BackupPolicy}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.BackupPolicy,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(BackupPolicyUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!BackupPolicyNameValidator.IsValid(options.BackupPolicy))
        {
            validationResult.Errors.Add(BackupPolicyNameValidator.ErrorMessage);
        }

        if (options.DailyBackupsToKeep is < 0 || options.WeeklyBackupsToKeep is < 0 || options.MonthlyBackupsToKeep is < 0)
        {
            validationResult.Errors.Add("--daily-backups-to-keep, --weekly-backups-to-keep, and --monthly-backups-to-keep must be zero or greater when provided.");
        }

        if (options.DailyBackupsToKeep is null &&
            options.WeeklyBackupsToKeep is null &&
            options.MonthlyBackupsToKeep is null &&
            options.Enabled is null)
        {
            validationResult.Errors.Add("At least one of --daily-backups-to-keep, --weekly-backups-to-keep, --monthly-backups-to-keep, or --enabled is required.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files backup policy could not be updated because of a resource conflict. Verify the backup policy state and retention values.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Azure NetApp Files backup policy. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The Azure NetApp Files account or backup policy was not found. Verify the account, backup policy, and resource group names.",
        _ => base.GetErrorMessage(ex)
    };

    public record BackupPolicyUpdateResult(NetAppFilesBackupPolicy BackupPolicy);
}