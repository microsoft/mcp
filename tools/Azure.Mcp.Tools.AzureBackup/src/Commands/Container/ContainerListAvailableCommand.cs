// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Container;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Container;

[CommandMetadata(
    Id = "b5d7e3f9-2a4c-4b6e-8d1f-9a3c7e5b2d4f",
    Name = "list-available",
    Title = "List Available Backup Containers",
    Description = "Lists storage accounts that a Recovery Services vault can register as Azure File share backup containers. Run container refresh first when the storage account is not yet discoverable. Only supported for Recovery Services vaults; use --storage-account to filter by account name or ARM resource ID.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerListAvailableCommand(ILogger<ContainerListAvailableCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<ContainerListAvailableOptions, ContainerListAvailableCommand.ContainerListAvailableCommandResult>(subscriptionResolver)
{
    private const string DefaultFilter = "backupManagementType eq 'AzureStorage'";
    private readonly ILogger<ContainerListAvailableCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(ContainerListAvailableOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.Filter is { Length: > 4096 })
        {
            validationResult.Errors.Add("The --filter value must not exceed 4096 characters.");
        }

        if (options.StorageAccount is { Length: > 2048 })
        {
            validationResult.Errors.Add("The --storage-account value must not exceed 2048 characters.");
        }

        if (VaultTypeResolver.IsDpp(options.VaultType))
        {
            validationResult.Errors.Add("Listing available containers is only supported for Recovery Services (RSV) vaults. Backup vaults (DPP) do not use protection containers.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ContainerListAvailableOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultAndWorkloadTags(context.Activity, options.VaultType ?? VaultTypeResolver.Rsv, null);

        var effectiveFilter = string.IsNullOrWhiteSpace(options.Filter) ? DefaultFilter : options.Filter;

        try
        {
            var containers = await _azureBackupService.ListAvailableContainersAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                effectiveFilter,
                options.StorageAccount,
                options.VaultType,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ContainerListAvailableCommandResult(containers),
                AzureBackupJsonContext.Default.ContainerListAvailableCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing available backup containers. Vault: {Vault}", options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record ContainerListAvailableCommandResult(List<ProtectableContainerInfo> Containers);
}
