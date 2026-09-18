// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.Backup;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.Backup;

[CommandMetadata(
    Id = "80cd0140-123f-474c-9fe5-08cbc65f6c23",
    Name = "update",
    Title = "Update Azure NetApp Files Backup",
    Description = "Updates the label of an Azure NetApp Files backup. Requires the account, backup vault, backup name, label, resource group, and subscription. Returns the updated backup details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupUpdateCommand(
    ILogger<BackupUpdateCommand> logger,
    INetAppFilesBackupService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupUpdateOptions, BackupUpdateCommand.BackupUpdateResult>(subscriptionResolver)
{
    private readonly ILogger<BackupUpdateCommand> _logger = logger;
    private readonly INetAppFilesBackupService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        BackupUpdateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var backup = await _service.UpdateBackupAsync(
                options.Account,
                options.BackupVault,
                options.Backup,
                options.Label,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupUpdateResult(backup),
                NetAppFilesJsonContext.Default.BackupUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error updating Azure NetApp Files backup. Account: {Account}, BackupVault: {BackupVault}, Backup: {Backup}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.BackupVault,
                options.Backup,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(BackupUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!BackupNameValidator.IsValidBackupVault(options.BackupVault))
        {
            validationResult.Errors.Add(BackupNameValidator.BackupVaultErrorMessage);
        }

        if (!BackupNameValidator.IsValidBackup(options.Backup))
        {
            validationResult.Errors.Add(BackupNameValidator.BackupErrorMessage);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files backup could not be updated because of a resource conflict. Verify the backup state.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating the Azure NetApp Files backup. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The Azure NetApp Files account, backup vault, or backup was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record BackupUpdateResult(NetAppFilesBackup Backup);
}
