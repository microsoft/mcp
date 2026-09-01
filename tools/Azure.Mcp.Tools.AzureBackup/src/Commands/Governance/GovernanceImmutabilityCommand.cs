// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Governance;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Governance;

[CommandMetadata(
    Id = "a0ac7596-9a80-4b53-b459-06f27598a2e2",
    Name = "immutability",
    Title = "Configure Vault Immutability",
    Description = """
        Configures the immutability state for a backup vault. --immutability-state 'Locked'
        is irreversible, 'Enabled' is an alias for 'Unlocked'. --immutability-type 'TimeBased'
        also requires --immutability-duration-days (30-36135) unless --immutability-state
        is 'Disabled'.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class GovernanceImmutabilityCommand(ILogger<GovernanceImmutabilityCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<GovernanceImmutabilityOptions, GovernanceImmutabilityCommand.GovernanceImmutabilityCommandResult>(subscriptionResolver)
{
    private readonly ILogger<GovernanceImmutabilityCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(GovernanceImmutabilityOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // Enum values are validated by the model binder. Extra semantic rules:
        //  * TimeBased requires a duration in [30, 36135] days, but only when the state
        //    is not Disabled (the service accepts and ignores duration when state=Disabled).
        //  * AsPerPolicy ignores duration.
        //  * Locked is IRREVERSIBLE - surfaced in the command description; we do not
        //    silently reject it here because the caller may legitimately want to lock.
        if (options.ImmutabilityType == AzureBackupImmutabilityType.TimeBased
            && options.ImmutabilityState != AzureBackupImmutabilityState.Disabled)
        {
            if (options.ImmutabilityDurationDays is null)
            {
                validationResult.Errors.Add("--immutability-duration-days is required when --immutability-type is 'TimeBased' and --immutability-state is not 'Disabled'.");
            }
            else if (options.ImmutabilityDurationDays < 30 || options.ImmutabilityDurationDays > 36135)
            {
                validationResult.Errors.Add("--immutability-duration-days must be between 30 and 36135 for TimeBased immutability.");
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GovernanceImmutabilityOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);

        try
        {
            var result = await _azureBackupService.ConfigureImmutabilityAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.ImmutabilityState,
                options.ImmutabilityType,
                options.ImmutabilityDurationDays,
                options.VaultType,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.GovernanceImmutabilityCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring immutability. Vault: {Vault}, State: {ImmutabilityState}",
                options.Vault, options.ImmutabilityState);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found. Verify the vault name and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Immutability state cannot be changed. It may already be locked.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record GovernanceImmutabilityCommandResult(OperationResult Result);
}