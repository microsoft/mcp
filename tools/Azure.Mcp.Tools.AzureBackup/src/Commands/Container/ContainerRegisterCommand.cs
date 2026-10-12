// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Container;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Container;

[CommandMetadata(
    Id = "c7e9f4a2-3b6d-4e8f-9a1c-2d5e7b9f0a34",
    Name = "register",
    Title = "Register Backup Container",
    Description = """
        Registers an Azure Storage account as an Azure File share backup container in a
        Recovery Services vault. This is the first step to protect Azure Files: register the
        storage account, then run 'azurebackup protectableitem inquire' to discover its file
        shares, then 'azurebackup protecteditem protect' to enable backup. Provide the storage
        account with --storage-account (bare name in the vault resource group, or a full ARM
        resource ID). By default a management lock is acquired on the storage account to prevent
        accidental deletion; pass --acquire-lock false to skip it. Registration is idempotent:
        if the storage account is already registered the tool returns its current state without
        re-registering. Only supported for Recovery Services vaults (RSV); Backup vaults (DPP)
        are not supported. Run 'azurebackup container refresh' first if the vault has not yet
        picked up the caller's Storage Account List Keys permission.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerRegisterCommand(ILogger<ContainerRegisterCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ContainerRegisterOptions, ContainerRegisterCommand.ContainerRegisterCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ContainerRegisterCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(ContainerRegisterOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.StorageAccount is { Length: > 2048 })
        {
            validationResult.Errors.Add("The --storage-account value must not exceed 2048 characters.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ContainerRegisterOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultAndWorkloadTags(context.Activity, VaultTypeResolver.Rsv, "AzureFileShare");

        var acquireLock = options.AcquireLock ?? true;

        try
        {
            var result = await _azureBackupService.RegisterContainerAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.StorageAccount,
                acquireLock,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ContainerRegisterCommandResult(result),
                AzureBackupJsonContext.Default.ContainerRegisterCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering backup container. Vault: {Vault}, StorageAccount: {StorageAccount}", options.Vault, options.StorageAccount);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed registering the container. Ensure the caller has the 'Backup Contributor' role on the vault and the vault system-assigned managed identity has 'Storage Account Backup Contributor' on the storage account. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"The storage account is already registered with a different vault, or a conflicting operation is in progress. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            $"The specified vault or storage account was not found. Verify --vault, --resource-group, --storage-account, and --subscription. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ContainerRegisterCommandResult(ContainerRegisterResult Registration);
}
