// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;

public sealed class ProtectableItemListCommand(ILogger<ProtectableItemListCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<ProtectableItemListOptions>()
{
    private const string CommandTitle = "List Protectable Items";
    private readonly ILogger<ProtectableItemListCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "9f6b0a1e-1c2d-4e5f-8a9b-7c6d5e4f3a21";
    public override string Name => "list";
    public override string Description =>
        """
        Lists items that can be backed up (protectable items) in a Recovery Services vault,
        such as SQL databases and SAP HANA databases discovered on registered VMs.
        Use this to find databases and workloads available for backup protection.
        Only supported for RSV vaults; DPP datasources are protected by ARM resource ID directly.
        Filter results by --workload-type (e.g., SQL, SAPHana) or --container.
        """;
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.WorkloadType);
        command.Options.Add(AzureBackupOptionDefinitions.Container);
    }

    protected override ProtectableItemListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkloadType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.WorkloadType.Name);
        options.Container = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Container.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        AzureBackupTelemetryTags.AddVaultAndWorkloadTags(context.Activity, options.VaultType ?? "rsv", options.WorkloadType);

        try
        {
            var result = await _azureBackupService.ListProtectableItemsAsync(
                options.Vault!,
                options.ResourceGroup!,
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

    internal record ProtectableItemListCommandResult(List<ProtectableItemInfo> Items);
}
