// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Vault.PrivateEndpoint;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault.PrivateEndpoint;

[CommandMetadata(
    Id = "d92f4e51-8b23-4c9a-a1e8-7d5f9b2c0a34",
    Name = "create",
    Title = "Create Private Endpoint for RSV",
    Description = """
        Creates a Private Endpoint (v2 experience) for a Recovery Services vault (RSV) in a customer VNet
        subnet. The command provisions the Microsoft.Network/privateEndpoints resource and, when
        --auto-approve is true, approves the resulting Private Endpoint Connection on the vault. Backup
        vaults (DPP) are not supported and return a NotSupportedException with guidance. The vault must
        have no protected items; RSV supports at most 12 Private Endpoints per vault. --group-id must be
        'AzureBackup' (primary region) or 'AzureBackup_secondary' (paired region / Cross-Region Restore).
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PrivateEndpointCreateCommand(
    ILogger<PrivateEndpointCreateCommand> logger,
    IAzureBackupService azureBackupService,
    ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<PrivateEndpointCreateOptions, PrivateEndpointCreateCommand.PrivateEndpointCreateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PrivateEndpointCreateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PrivateEndpointCreateOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.SetTag(AzureBackupTelemetryTags.PrivateEndpointAction, "create");

        try
        {
            var result = await _azureBackupService.CreatePrivateEndpointAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.PrivateEndpointName,
                options.VnetSubnetId,
                options.GroupId ?? "AzureBackup",
                options.Location,
                options.AutoApprove ?? false,
                options.VaultType,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.PrivateEndpointCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Private Endpoint. Vault: {Vault}, Name: {PrivateEndpointName}",
                options.Vault, options.PrivateEndpointName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        NotSupportedException nse => nse.Message,
        InvalidOperationException ioe => ioe.Message,
        ArgumentException argEx => argEx.Message,
        UnauthorizedAccessException => "Authorization failed. Verify your RBAC permissions on the vault and the target subnet, or specify --vault-type to skip auto-detection.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault, resource group, or subnet not found. Verify --vault, --resource-group, and --vnet-subnet-id.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Bad request creating Private Endpoint. Ensure the subnet has 'privateEndpointNetworkPolicies=Disabled' and lives in a region compatible with the vault's group-id. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating Private Endpoint. You need Microsoft.Network/privateEndpoints/write on the target resource group. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"A resource conflict occurred creating Private Endpoint. Verify a Private Endpoint with the same name does not already exist. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        NotSupportedException => HttpStatusCode.BadRequest,
        InvalidOperationException => HttpStatusCode.BadRequest,
        ArgumentException or FormatException => HttpStatusCode.BadRequest,
        UnauthorizedAccessException => HttpStatusCode.Forbidden,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record PrivateEndpointCreateCommandResult(PrivateEndpointConnectionInfo Connection);
}
