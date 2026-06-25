// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;

[CommandMetadata(
    Id = "9f6b0a1e-1c2d-4e5f-8a9b-7c6d5e4f3a21",
    Name = "list",
    Title = "List Protectable Items",
    Description = """
        Lists items that can be backed up (protectable items) in a Recovery Services vault,
        such as SQL databases and SAP HANA databases discovered on registered VMs.
        Use this to find databases and workloads available for backup protection.
        Only supported for RSV vaults; DPP datasources are protected by ARM resource ID directly.
        Filter results by --workload-type (e.g., SQL, SAPHana) or --container.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ProtectableItemListCommand(ILogger<ProtectableItemListCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<ProtectableItemListOptions, ProtectableItemListCommand.ProtectableItemListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ProtectableItemListCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(ProtectableItemListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // NEW-4: reject unknown --workload-type at the command boundary so it surfaces
        // as a 400 ValidationError instead of leaking the inner ArgumentException
        // from the service layer as a 500.
        //
        // Read the value directly (no HasOptionResult gate) so whitespace-only inputs --
        // which System.CommandLine may report as "no result" -- still fail validation
        // here instead of slipping past and being rejected by the service layer.
        if (options.WorkloadType != null && !WorkloadTypeNormalizer.IsSupported(options.WorkloadType))
        {
            validationResult.Errors.Add(WorkloadTypeNormalizer.FormatUnknownMessage(options.WorkloadType));
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ProtectableItemListOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultAndWorkloadTags(context.Activity, options.VaultType ?? "rsv", options.WorkloadType);

        try
        {
            var result = await _azureBackupService.ListProtectableItemsAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.WorkloadType,
                options.Container,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.ProtectableItemListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing protectable items. Vault: {Vault}", options.Vault);
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

    public sealed record ProtectableItemListCommandResult(List<ProtectableItemInfo> Items);
}
