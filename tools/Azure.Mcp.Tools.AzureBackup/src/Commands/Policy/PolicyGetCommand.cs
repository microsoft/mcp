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

/// <summary>
/// Consolidated policy command: when --policy is supplied returns a single policy's details;
/// otherwise lists all policies in the vault.
/// </summary>
[CommandMetadata(
    Id = "5f7ef3ae-72f3-4fe8-bd1e-ea56e4db86df",
    Name = "get",
    Title = "Get Backup Policy",
    Description = """
        Retrieves backup policy information. When --policy is specified, returns detailed
        information about a single policy including datasource types and protected items count.
        When omitted, lists all backup policies configured in the vault.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class PolicyGetCommand(ILogger<PolicyGetCommand> logger, IAzureBackupService azureBackupService, ISubscriptionResolver subscriptionResolver)
    : BaseAzureBackupCommand<PolicyGetOptions, PolicyGetCommand.PolicyGetCommandResult>(subscriptionResolver)
{
    private readonly ILogger<PolicyGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PolicyGetOptions options, CancellationToken cancellationToken)
    {
        AzureBackupTelemetryTags.AddSubscriptionTag(context.Activity, options.Subscription);
        AzureBackupTelemetryTags.AddVaultTags(context.Activity, options.VaultType);
        context.Activity?.AddTag(AzureBackupTelemetryTags.OperationScope, string.IsNullOrEmpty(options.Policy) ? "list" : "single");

        try
        {
            if (!string.IsNullOrEmpty(options.Policy))
            {
                var policy = await _azureBackupService.GetPolicyAsync(
                    options.Vault,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Policy,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new([policy]),
                    AzureBackupJsonContext.Default.PolicyGetCommandResult);
            }
            else
            {
                var policies = await _azureBackupService.ListPoliciesAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(policies),
                    AzureBackupJsonContext.Default.PolicyGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policy/policies. Policy: {Policy}, Vault: {Vault}", options.Policy, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Policy not found. Verify the policy name and vault.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record PolicyGetCommandResult(List<BackupPolicyInfo> Policies);
}
