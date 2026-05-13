// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

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
public sealed class ProtectedItemProtectCommand(ILogger<ProtectedItemProtectCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<ProtectedItemProtectOptions>()
{
    private readonly ILogger<ProtectedItemProtectCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.DatasourceId.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.Policy.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.Container);
        command.Options.Add(AzureBackupOptionDefinitions.DatasourceType);
        command.Options.Add(AzureBackupOptionDefinitions.AksSnapshotResourceGroup);
        command.Options.Add(AzureBackupOptionDefinitions.AksIncludedNamespaces);
        command.Options.Add(AzureBackupOptionDefinitions.AksExcludedNamespaces);
        command.Options.Add(AzureBackupOptionDefinitions.AksLabelSelectors);
        command.Options.Add(AzureBackupOptionDefinitions.AksIncludeClusterScopeResources);
    }

    protected override ProtectedItemProtectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DatasourceId = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.DatasourceId.Name);
        options.Policy = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Policy.Name);
        options.Container = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Container.Name);
        options.DatasourceType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.DatasourceType.Name);
        options.AksSnapshotResourceGroup = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.AksSnapshotResourceGroup.Name);
        options.AksIncludedNamespaces = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.AksIncludedNamespaces.Name);
        options.AksExcludedNamespaces = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.AksExcludedNamespaces.Name);
        options.AksLabelSelectors = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.AksLabelSelectors.Name);
        options.AksIncludeClusterScopeResources = parseResult.GetValueOrDefault<bool>(AzureBackupOptionDefinitions.AksIncludeClusterScopeResources.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.AddTag(AzureBackupTelemetryTags.DatasourceType, AzureBackupTelemetryTags.NormalizeWorkloadType(options.DatasourceType));

        try
        {
            var result = await _azureBackupService.ProtectItemAsync(
                options.Vault!,
                options.ResourceGroup!,
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

    internal record ProtectedItemProtectCommandResult(ProtectResult Result);
}
