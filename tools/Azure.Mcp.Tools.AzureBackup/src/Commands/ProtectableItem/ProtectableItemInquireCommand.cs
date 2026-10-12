// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;

[CommandMetadata(
    Id = "d8f0a5b3-4c7e-4f9a-8b2d-3e6f8c0a1b45",
    Name = "inquire",
    Title = "Inquire Backup Container",
    Description = """
        Triggers discovery (inquiry) of backup-able items inside a registered RSV protection
        container, such as the Azure File shares in a registered storage account. Run this after
        'azurebackup container register' and before 'azurebackup protectableitem list' so the
        vault enumerates the file shares available for protection. Identify the container with
        --container (the RSV container name) or --storage-account (bare name in the vault
        resource group, or a full ARM resource ID) - exactly one is required. The Azure API is
        an asynchronous fire-and-forget request that returns HTTP 202 Accepted with no body;
        this tool reports that acceptance status rather than a discovered item list. Follow up
        with 'azurebackup protectableitem list' to enumerate discovered shares. Only supported
        for Recovery Services vaults (RSV); Backup vaults (DPP) are not supported.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ProtectableItemInquireCommand(ILogger<ProtectableItemInquireCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ProtectableItemInquireOptions, ProtectableItemInquireCommand.ProtectableItemInquireCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ProtectableItemInquireCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(ProtectableItemInquireOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var hasContainer = !string.IsNullOrWhiteSpace(options.Container);
        var hasStorageAccount = !string.IsNullOrWhiteSpace(options.StorageAccount);

        if (hasContainer && hasStorageAccount)
        {
            validationResult.Errors.Add("--container and --storage-account are mutually exclusive. Specify only one.");
        }
        else if (!hasContainer && !hasStorageAccount)
        {
            validationResult.Errors.Add("Specify either --container or --storage-account to identify the container to inquire.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ProtectableItemInquireOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultAndWorkloadTags(context.Activity, VaultTypeResolver.Rsv, "AzureFileShare");

        try
        {
            var result = await _azureBackupService.InquireContainerAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.Container,
                options.StorageAccount,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ProtectableItemInquireCommandResult(result),
                AzureBackupJsonContext.Default.ProtectableItemInquireCommandResult);
            context.Response.Status = HttpStatusCode.Accepted;
            context.Response.Message = "Accepted";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inquiring backup container. Vault: {Vault}", options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        KeyNotFoundException notFoundEx => notFoundEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed inquiring the container. Ensure the caller has the 'Backup Contributor' role on the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        KeyNotFoundException => HttpStatusCode.NotFound,
        _ => base.GetStatusCode(ex)
    };

    public sealed record ProtectableItemInquireCommandResult(InquireResult Inquiry);
}
