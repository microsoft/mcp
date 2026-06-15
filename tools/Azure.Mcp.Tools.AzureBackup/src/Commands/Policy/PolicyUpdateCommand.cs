// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Policy;

[CommandMetadata(
    Id = "a3f7d2c1-9e84-4b6a-8d3c-5f1e7a2b9c04",
    Name = "update",
    Title = "Update Backup Policy",
    Description = "Modifies an existing RSV backup policy. Updates the backup schedule time and daily retention days for VM, SQL, SAP HANA, and file share workload policies. The named policy must already exist in the vault.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class PolicyUpdateCommand(ILogger<PolicyUpdateCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<PolicyUpdateOptions, PolicyUpdateCommand.PolicyUpdateCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PolicyUpdateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PolicyUpdateOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);

        try
        {
            var result = await _azureBackupService.UpdatePolicyAsync(
                options.Vault,
                options.ResourceGroup,
                options.Subscription!,
                options.Policy,
                options.VaultType,
                options.ScheduleTime,
                options.DailyRetentionDays,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.PolicyUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating policy. Policy: {Policy}, Vault: {Vault}",
                options.Policy, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Policy or vault not found. Verify the policy name, vault name, and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the policy. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    public sealed record PolicyUpdateCommandResult(OperationResult Result);
}
