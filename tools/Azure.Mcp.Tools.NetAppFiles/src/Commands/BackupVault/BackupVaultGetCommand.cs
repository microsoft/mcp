// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Options.BackupVault;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Azure.Mcp.Tools.NetAppFiles.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Commands.BackupVault;

[CommandMetadata(
    Id = "1c426714-9850-4b2e-ae8f-8332af65bde2",
    Name = "get",
    Title = "Get Azure NetApp Files Backup Vault",
    Description = "Gets an Azure NetApp Files backup vault by name from an account. Requires the account, backup vault, resource group, and subscription. Returns the backup vault details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupVaultGetCommand(
    ILogger<BackupVaultGetCommand> logger,
    INetAppFilesBackupVaultService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupVaultGetOptions, BackupVaultGetCommand.BackupVaultGetResult>(subscriptionResolver)
{
    private readonly ILogger<BackupVaultGetCommand> _logger = logger;
    private readonly INetAppFilesBackupVaultService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        BackupVaultGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var backupVault = await _service.GetBackupVaultAsync(
                options.Account,
                options.BackupVault,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupVaultGetResult(backupVault),
                NetAppFilesJsonContext.Default.BackupVaultGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting Azure NetApp Files backup vault. Account: {Account}, BackupVault: {BackupVault}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                options.Account,
                options.BackupVault,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(BackupVaultGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!AccountNameValidator.IsValid(options.Account))
        {
            validationResult.Errors.Add(AccountNameValidator.ErrorMessage);
        }

        if (!BackupVaultNameValidator.IsValid(options.BackupVault))
        {
            validationResult.Errors.Add(BackupVaultNameValidator.ErrorMessage);
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "The resource group was not found. Verify it exists and you have access.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the Azure NetApp Files backup vault. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group, Azure NetApp Files account, or backup vault was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record BackupVaultGetResult(NetAppFilesBackupVault BackupVault);
}
