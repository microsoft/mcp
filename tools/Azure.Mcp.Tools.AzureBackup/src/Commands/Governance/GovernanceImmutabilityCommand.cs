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
        Configures the immutability state for a backup vault. States include 'Disabled', 'Enabled',
        or 'Locked'. Warning: 'Locked' state is irreversible.
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

        if (!string.IsNullOrEmpty(options.ImmutabilityState) &&
            !options.ImmutabilityState.Equals("Disabled", StringComparison.OrdinalIgnoreCase) &&
            !options.ImmutabilityState.Equals("Enabled", StringComparison.OrdinalIgnoreCase) &&
            !options.ImmutabilityState.Equals("Locked", StringComparison.OrdinalIgnoreCase))
        {
            validationResult.Errors.Add("--immutability-state must be 'Disabled', 'Enabled', or 'Locked'. Warning: 'Locked' is irreversible.");
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
                options.ImmutabilityState!,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
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
