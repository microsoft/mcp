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
    Id = "a4c6d2f8-1e5b-4d7a-9c3f-8e2b6a1d5f70",
    Name = "refresh",
    Title = "Refresh Backup Containers",
    Description = """
        Triggers the RSV RefreshContainers (discovery) operation on a Recovery Services vault.
        Use this before registering a storage account for Azure File share backup so the vault
        picks up the caller's new/changed permissions on Storage Account List Keys. Also useful
        after enabling the vault system-assigned managed identity or assigning the
        'Storage Account Backup Contributor' role. Backup management type defaults to
        'AzureStorage' (Azure Files); select 'AzureIaasVM' or 'AzureWorkload' to discover
        virtual machine or in-guest workload containers instead. RSV only; DPP vaults are not
        supported. The Azure API is a fire-and-forget POST that returns HTTP 202 Accepted with no body;
        this tool reports that acceptance status rather than a discovered container list. Follow
        up with 'azurebackup protectableitem list' or 'protectableitem inquire' to enumerate
        newly-discovered shares.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerRefreshCommand(ILogger<ContainerRefreshCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ContainerRefreshOptions, ContainerRefreshCommand.ContainerRefreshCommandResult>(subscriptionResolver)
{
    private const string DefaultBackupManagementType = "AzureStorage";

    private readonly ILogger<ContainerRefreshCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(ContainerRefreshOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (!string.IsNullOrWhiteSpace(options.BackupManagementType) &&
            !IsSupportedBackupManagementType(options.BackupManagementType))
        {
            validationResult.Errors.Add("--backup-management-type must be 'AzureStorage', 'AzureIaasVM', or 'AzureWorkload'.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ContainerRefreshOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultAndWorkloadTags(context.Activity, VaultTypeResolver.Rsv, null);

        var backupManagementType = GetBackupManagementType(options.BackupManagementType);

        try
        {
            await _azureBackupService.RefreshContainersAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                backupManagementType,
                options.Tenant,
                cancellationToken);

            var result = new ContainerRefreshCommandResult(
                Status: "Accepted",
                Vault: options.Vault,
                BackupManagementType: backupManagementType,
                Message: "Container discovery request accepted. The vault will asynchronously enumerate matching resources. Poll 'azurebackup protectableitem list' to see newly-discovered items.");

            context.Response.Results = ResponseResult.Create(
                result,
                AzureBackupJsonContext.Default.ContainerRefreshCommandResult);
            context.Response.Status = HttpStatusCode.Accepted;
            context.Response.Message = "Accepted";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing containers. Vault: {Vault}, BackupManagementType: {BackupManagementType}", options.Vault, backupManagementType);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        NotSupportedException notSupEx => notSupEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed refreshing containers. Ensure the caller has the 'Backup Contributor' role on the vault and the vault system-assigned managed identity has 'Storage Account Backup Contributor' on the target subscription or storage accounts. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            $"The specified Recovery Services vault was not found. Verify --vault, --resource-group, and --subscription. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        NotSupportedException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    private static bool IsSupportedBackupManagementType(string value) =>
        value.Equals("AzureStorage", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("AzureIaasVM", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("AzureWorkload", StringComparison.OrdinalIgnoreCase);

    private static string GetBackupManagementType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DefaultBackupManagementType;
        }

        if (value.Equals("AzureStorage", StringComparison.OrdinalIgnoreCase))
        {
            return "AzureStorage";
        }

        if (value.Equals("AzureIaasVM", StringComparison.OrdinalIgnoreCase))
        {
            return "AzureIaasVM";
        }

        if (value.Equals("AzureWorkload", StringComparison.OrdinalIgnoreCase))
        {
            return "AzureWorkload";
        }

        return value;
    }

    public sealed record ContainerRefreshCommandResult(
        string Status,
        string Vault,
        string BackupManagementType,
        string Message);
}
