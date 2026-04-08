// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Policy;

/// <summary>
/// Consolidated policy command: when --policy is supplied returns a single policy's details;
/// otherwise lists all policies in the vault.
/// </summary>
public sealed class PolicyGetCommand(ILogger<PolicyGetCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<PolicyGetOptions>()
{
    private const string CommandTitle = "Get Backup Policy";
    private readonly ILogger<PolicyGetCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "5f7ef3ae-72f3-4fe8-bd1e-ea56e4db86df";
    public override string Name => "get";
    public override string Description =>
        """
        Retrieves backup policy information. When --policy is specified, returns detailed
        information about a single policy including datasource types and protected items count.
        When omitted, lists all backup policies configured in the vault.
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
        command.Options.Add(AzureBackupOptionDefinitions.Policy.AsOptional());
    }

    protected override PolicyGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Policy = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Policy.Name);
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
            if (!string.IsNullOrEmpty(options.Policy))
            {
                var policy = await _azureBackupService.GetPolicyAsync(
                    options.Vault!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Policy,
                    options.VaultType,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new PolicyGetCommandResult([policy]),
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
                    new PolicyGetCommandResult(policies),
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

    internal record PolicyGetCommandResult([property: JsonPropertyName("policies")] List<BackupPolicyInfo> Policies);
}
