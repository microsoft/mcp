// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Container;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Container;

/// <summary>
/// Looks up a single RSV protection container by name or by storage account.
/// A 404 from the vault is surfaced as a successful response with 'registered: false'
/// so callers can drive idempotent register/refresh flows without treating "not registered
/// yet" as an error.
/// </summary>
[CommandMetadata(
    Id = "b7d4e9a2-3f1c-4a5b-8d6e-2c1f9b4a7e83",
    Name = "get",
    Title = "Get RSV Protection Container",
    Description = """
        Retrieves a single Recovery Services vault (RSV) protection container by name or by storage
        account. Supply either --container (the fully qualified RSV container name) or --storage-account
        (a bare storage account name or ARM resource ID); the container name is derived automatically
        for storage accounts. When the container is not registered the response is HTTP 200 with
        'registered: false' and 'container: null' — this is the idempotency signal for register/refresh
        callers. Only supported for Recovery Services vaults (RSV); Backup vaults (DPP) return HTTP 400.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ContainerGetCommand(ILogger<ContainerGetCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<ContainerGetOptions, ContainerGetCommand.ContainerGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ContainerGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override void ValidateOptions(ContainerGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (VaultTypeResolver.IsVaultTypeSpecified(options.VaultType) && VaultTypeResolver.IsDpp(options.VaultType))
        {
            validationResult.Errors.Add(
                "Backup vaults (DPP) do not use protection containers. This command is only supported for Recovery Services vaults (RSV).");
        }

        var hasContainer = !string.IsNullOrWhiteSpace(options.Container);
        var hasStorageAccount = !string.IsNullOrWhiteSpace(options.StorageAccount);

        if (hasContainer == hasStorageAccount)
        {
            validationResult.Errors.Add(
                hasContainer
                    ? "Specify exactly one of --container or --storage-account (they are mutually exclusive)."
                    : "Specify exactly one of --container or --storage-account.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ContainerGetOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);

        try
        {
            var containerName = ResolveContainerName(options);

            var container = await _azureBackupService.GetContainerAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                containerName,
                options.VaultType,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(container is not null, container),
                AzureBackupJsonContext.Default.ContainerGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting container. Vault: {Vault}, ResourceGroup: {ResourceGroup}, Container: {Container}, StorageAccount: {StorageAccount}",
                options.Vault,
                options.ResourceGroup,
                options.Container,
                options.StorageAccount);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static string ResolveContainerName(ContainerGetOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Container))
        {
            return options.Container!.Trim();
        }

        var input = options.StorageAccount!.Trim();

        // Bare storage account name → assume same resource group as the vault.
        if (!input.StartsWith('/'))
        {
            return $"StorageContainer;Storage;{options.ResourceGroup};{input}";
        }

        // Full ARM resource ID.
        var resourceId = new ResourceIdentifier(input);
        return $"StorageContainer;Storage;{resourceId.ResourceGroupName};{resourceId.Name}";
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed getting the RSV protection container. Ensure the caller has the 'Backup Reader' or 'Backup Contributor' role on the vault.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found. Verify the vault name and resource group.",
        NotSupportedException => ex.Message,
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        NotSupportedException => HttpStatusCode.BadRequest,
        FormatException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record ContainerGetCommandResult(bool Registered, BackupContainerInfo? Container);
}
