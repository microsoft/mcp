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
    Id = "c84e8e9a-8a57-4d15-a476-e2b86b0a84ce",
    Name = "get",
    Title = "Get Azure NetApp Files Backup",
    Description = "Gets an Azure NetApp Files backup by name from a backup vault. Requires the account, backup vault, backup name, resource group, and subscription. Returns the backup details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupGetCommand(
    ILogger<BackupGetCommand> logger,
    INetAppFilesBackupService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupGetOptions, BackupGetCommand.BackupGetResult>(subscriptionResolver)
{
    private readonly ILogger<BackupGetCommand> _logger = logger;
    private readonly INetAppFilesBackupService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        BackupGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var backup = await _service.GetBackupAsync(
                options.Account,
                options.BackupVault,
                options.Backup,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupGetResult(backup),
                NetAppFilesJsonContext.Default.BackupGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting Azure NetApp Files backup. Account: {Account}, BackupVault: {BackupVault}, Backup: {Backup}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.BackupVault,
                options.Backup,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(BackupGetOptions options, ValidationResult validationResult)
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
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the Azure NetApp Files backup. Verify that you have the required RBAC permissions.",
        RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The Azure NetApp Files account, backup vault, or backup was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record BackupGetResult(NetAppFilesBackup Backup);
}
