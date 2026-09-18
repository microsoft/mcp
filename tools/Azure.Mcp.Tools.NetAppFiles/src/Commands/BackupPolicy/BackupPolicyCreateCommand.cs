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
    Id = "53932516-cdd8-4a8c-9c87-3b42338444ef",
    Name = "create",
    Title = "Create Azure NetApp Files Backup Policy",
    Description = "Creates a backup policy in an Azure NetApp Files account. Requires the account, backup policy, location, daily, weekly, and monthly retention counts, enabled state, resource group, and subscription. Returns the created backup policy details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupPolicyCreateCommand(
    ILogger<BackupPolicyCreateCommand> logger,
    INetAppFilesBackupPolicyService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupPolicyCreateOptions, BackupPolicyCreateCommand.BackupPolicyCreateResult>(subscriptionResolver)
{
    private readonly ILogger<BackupPolicyCreateCommand> _logger = logger;
    private readonly INetAppFilesBackupPolicyService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        BackupPolicyCreateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var backupPolicy = await _service.CreateBackupPolicyAsync(
                options.Account,
                options.BackupPolicy,
                options.Location,
                options.DailyBackupsToKeep,
                options.WeeklyBackupsToKeep,
                options.MonthlyBackupsToKeep,
                options.Enabled!.Value,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupPolicyCreateResult(backupPolicy),
                NetAppFilesJsonContext.Default.BackupPolicyCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating Azure NetApp Files backup policy. Account: {Account}, BackupPolicy: {BackupPolicy}, ResourceGroup: {ResourceGroup}, Location: {Location}, Subscription: {Subscription}",
                options.Account,
                options.BackupPolicy,
                options.ResourceGroup,
                options.Location,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(BackupPolicyCreateOptions options, ValidationResult validationResult)
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

        if (options.DailyBackupsToKeep < 0 || options.WeeklyBackupsToKeep < 0 || options.MonthlyBackupsToKeep < 0)
        {
            validationResult.Errors.Add("--daily-backups-to-keep, --weekly-backups-to-keep, and --monthly-backups-to-keep must be zero or greater.");
        }

        if (options.DailyBackupsToKeep == 0 && options.WeeklyBackupsToKeep == 0 && options.MonthlyBackupsToKeep == 0)
        {
            validationResult.Errors.Add("At least one backup retention count must be greater than zero.");
        }

        if (options.Enabled is null)
        {
            validationResult.Errors.Add("--enabled is required.");
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files backup policy could not be created because of a resource conflict. Verify the account, backup policy name, and resource state.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating the Azure NetApp Files backup policy. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group or Azure NetApp Files account was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record BackupPolicyCreateResult(NetAppFilesBackupPolicy BackupPolicy);
}
