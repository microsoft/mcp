// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault;

/// <summary>
/// Consolidated vault command: when --vault is supplied returns a single vault's details;
/// otherwise lists all vaults in the subscription (optionally filtered by --vault-type).
/// </summary>
public sealed class VaultGetCommand(ILogger<VaultGetCommand> logger, IAzureBackupService azureBackupService) : SubscriptionCommand<BaseAzureBackupOptions>()
{
    private const string CommandTitle = "Get Backup Vault";
    private readonly ILogger<VaultGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "4a1084d5-50d9-489f-9e4c-acc594441b1f";
    public override string Name => "get";
    public override string Description =>
        """
        Retrieves backup vault information. When --vault and --resource-group are specified,
        returns detailed information about a single vault including type, location, SKU, and
        storage redundancy. When omitted, lists all backup vaults (RSV and Backup vaults) in
        the subscription, optionally filtered by --vault-type ('rsv' or 'dpp').
        Note: --resource-group is only used when --vault is specified; listing vaults always
        returns all vaults in the subscription regardless of resource group.
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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(AzureBackupOptionDefinitions.Vault.AsOptional());
        command.Options.Add(AzureBackupOptionDefinitions.VaultType);
        command.Validators.Add(commandResult =>
        {
            if (commandResult.HasOptionResult(AzureBackupOptionDefinitions.Vault.Name) &&
                !commandResult.HasOptionResult(OptionDefinitions.Common.ResourceGroup.Name))
            {
                commandResult.AddError("--resource-group is required when --vault is specified.");
            }

            if (commandResult.HasOptionResult(AzureBackupOptionDefinitions.VaultType.Name))
            {
                var value = commandResult.GetValue<string>(AzureBackupOptionDefinitions.VaultType.Name);
                if (!string.IsNullOrEmpty(value) &&
                    !value.Equals("rsv", StringComparison.OrdinalIgnoreCase) &&
                    !value.Equals("dpp", StringComparison.OrdinalIgnoreCase))
                {
                    commandResult.AddError("--vault-type must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
                }
            }
        });
    }

    protected override BaseAzureBackupOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Vault = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Vault.Name);
        options.VaultType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.VaultType.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            if (!string.IsNullOrEmpty(options.Vault))
            {
                var vault = await _azureBackupService.GetVaultAsync(
                    options.Vault,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new([vault]),
                    AzureBackupJsonContext.Default.VaultGetCommandResult);
            }
            else
            {
                var vaults = await _azureBackupService.ListVaultsAsync(
                    options.Subscription!,
                    options.ResourceGroup,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(vaults),
                    AzureBackupJsonContext.Default.VaultGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vault(s). Vault: {Vault}, ResourceGroup: {ResourceGroup}",
                options.Vault, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => "Vault not found. Verify the vault name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found. Verify the vault name and resource group.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VaultGetCommandResult(List<BackupVaultInfo> Vaults);
}
