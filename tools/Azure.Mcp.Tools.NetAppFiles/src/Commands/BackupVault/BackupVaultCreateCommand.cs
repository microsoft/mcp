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
    Id = "6da99182-09ab-4f09-bce1-0cd561b13547",
    Name = "create",
    Title = "Create Azure NetApp Files Backup Vault",
    Description = "Creates a backup vault in an Azure NetApp Files account. Requires the account, backup vault, location, resource group, and subscription. Returns the created backup vault details.",
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class BackupVaultCreateCommand(
    ILogger<BackupVaultCreateCommand> logger,
    INetAppFilesBackupVaultService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BackupVaultCreateOptions, BackupVaultCreateCommand.BackupVaultCreateResult>(subscriptionResolver)
{
    private readonly ILogger<BackupVaultCreateCommand> _logger = logger;
    private readonly INetAppFilesBackupVaultService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        BackupVaultCreateOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var backupVault = await _service.CreateBackupVaultAsync(
                options.Account,
                options.BackupVault,
                options.Location,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new BackupVaultCreateResult(backupVault),
                NetAppFilesJsonContext.Default.BackupVaultCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error creating Azure NetApp Files backup vault. Account: {Account}, BackupVault: {BackupVault}, ResourceGroup: {ResourceGroup}, Location: {Location}, Subscription: {Subscription}",
                options.Account,
                options.BackupVault,
                options.ResourceGroup,
                options.Location,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public override void ValidateOptions(BackupVaultCreateOptions options, ValidationResult validationResult)
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
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Conflict =>
            "The Azure NetApp Files backup vault could not be created because of a resource conflict. Verify the account, backup vault name, and resource state.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating the Azure NetApp Files backup vault. Verify that you have the required RBAC permissions.",
        Azure.RequestFailedException requestFailedException when requestFailedException.Status == (int)HttpStatusCode.NotFound =>
            "The resource group or Azure NetApp Files account was not found.",
        _ => base.GetErrorMessage(ex)
    };

    public record BackupVaultCreateResult(NetAppFilesBackupVault BackupVault);
}
