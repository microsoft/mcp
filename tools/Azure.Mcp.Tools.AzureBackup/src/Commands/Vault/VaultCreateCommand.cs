// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Vault;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault;

[CommandMetadata(
    Id = "1dccdb24-d81c-4bde-9437-577a7bd0cf09",
    Name = "create",
    Title = "Create Backup Vault",
    Description = """
        Creates a new backup vault. Specify --vault-type as 'rsv' for a Recovery Services vault
        or 'dpp' for a Backup vault (Data Protection). For DPP vaults a System-Assigned
        Managed Identity is enabled by default so the vault can authenticate to protected
        datasources (storage accounts, disks, PG Flex, etc.) - change later with
        'azurebackup vault update --identity-type ...' if needed. Returns the created
        vault details.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class VaultCreateCommand(ILogger<VaultCreateCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<VaultCreateOptions, VaultCreateCommand.VaultCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<VaultCreateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(VaultCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.VaultType is null)
        {
            validationResult.Errors.Add("--vault-type must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
        }

    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, VaultCreateOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);

        try
        {
            var result = await _azureBackupService.CreateVaultAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.VaultType ?? throw new InvalidOperationException("Vault type is required."),
                options.Location!,
                options.Sku,
                options.StorageType,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.VaultCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vault. Vault: {Vault}, ResourceGroup: {ResourceGroup}, Location: {Location}",
                options.Vault, options.ResourceGroup, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A vault with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record VaultCreateCommandResult(VaultCreateResult Vault);
}
