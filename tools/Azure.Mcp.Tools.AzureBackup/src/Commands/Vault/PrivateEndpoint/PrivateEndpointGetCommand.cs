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
    Id = "9f7b1e30-a58c-4b26-8f14-6c9d3a72e8b1",
    Name = "get",
    Title = "Get Private Endpoint Connection(s) for RSV",
    Description = """
        Retrieves Private Endpoint Connections (PECs) attached to a Recovery Services vault (RSV). When
        --private-endpoint-name is provided, returns that single connection. When omitted, lists every
        PEC on the vault. Backup vaults (DPP) are not supported.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PrivateEndpointGetCommand(
    ILogger<PrivateEndpointGetCommand> logger,
    IAzureBackupService azureBackupService,
    ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<PrivateEndpointGetOptions, PrivateEndpointGetCommand.PrivateEndpointGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PrivateEndpointGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PrivateEndpointGetOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.SetTag(AzureBackupTelemetryTags.PrivateEndpointAction, string.IsNullOrEmpty(options.PrivateEndpointName) ? "list" : "get");

        try
        {
            if (string.IsNullOrEmpty(options.PrivateEndpointName))
            {
                var list = await _azureBackupService.ListPrivateEndpointsAsync(
                    options.Vault, options.ResourceGroup, options.Subscription!,
                    options.VaultType, options.Tenant, options.RetryPolicy, cancellationToken);
                context.Response.Results = ResponseResult.Create(
                    new(Connections: list),
                    AzureBackupJsonContext.Default.PrivateEndpointGetCommandResult);
            }
            else
            {
                var single = await _azureBackupService.GetPrivateEndpointAsync(
                    options.Vault, options.ResourceGroup, options.Subscription!,
                    options.PrivateEndpointName!,
                    options.VaultType, options.Tenant, options.RetryPolicy, cancellationToken);
                context.Response.Results = ResponseResult.Create(
                    new(Connections: [single]),
                    AzureBackupJsonContext.Default.PrivateEndpointGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Private Endpoint Connection(s). Vault: {Vault}, Name: {PrivateEndpointName}",
                options.Vault, options.PrivateEndpointName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        NotSupportedException nse => nse.Message,
        UnauthorizedAccessException => "Authorization failed. Verify your RBAC permissions on the vault, or specify --vault-type to skip auto-detection.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault or Private Endpoint Connection not found. Verify --vault, --resource-group, and --private-endpoint-name.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        NotSupportedException => HttpStatusCode.BadRequest,
        UnauthorizedAccessException => HttpStatusCode.Forbidden,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record PrivateEndpointGetCommandResult(List<PrivateEndpointConnectionInfo> Connections);
}
