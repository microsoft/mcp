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
    Id = "7a6fc193-ca3c-4309-97c5-ee1e7fe90e69",
    Name = "protect",
    Title = "Protect Resource",
    Description = """
        Enables or configures backup protection for an Azure resource by creating a
        protected item or backup instance. Protects VMs, disks, file shares, SQL databases,
        SAP HANA databases, and other supported datasources.
        For VMs: pass the VM ARM resource ID as --datasource-id.
        For workloads (SQL/HANA): pass the protectable item name from 'protectableitem list'
        as --datasource-id (e.g., 'SAPHanaDatabase;instance;dbname'), and specify --container.
        Requires a backup policy name via --policy. The operation is asynchronous;
        use 'azurebackup job get' to monitor the protection job progress.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ProtectedItemProtectCommand(ILogger<ProtectedItemProtectCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<ProtectedItemProtectOptions, ProtectedItemProtectCommand.ProtectedItemProtectCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ProtectedItemProtectCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ProtectedItemProtectOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.AddTag(AzureBackupTelemetryTags.DatasourceType, AzureBackupTelemetryTags.NormalizeWorkloadType(options.DatasourceType));

        try
        {
            var result = await _azureBackupService.ProtectItemAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.DatasourceId!,
                options.Policy!,
                options.VaultType,
                options.Container,
                options.DatasourceType,
                options.AksIncludedNamespaces,
                options.AksExcludedNamespaces,
                options.AksLabelSelectors,
                options.AksIncludeClusterScopeResources ? "true" : null,
                options.AksSnapshotResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error protecting item. DatasourceId: {DatasourceId}, Vault: {Vault}",
                options.DatasourceId, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed protecting the resource. Ensure the caller has Backup Contributor role. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "This resource is already protected. Use 'azurebackup protecteditem get' to view its status.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ProtectedItemProtectCommandResult(ProtectResult Result);
}
