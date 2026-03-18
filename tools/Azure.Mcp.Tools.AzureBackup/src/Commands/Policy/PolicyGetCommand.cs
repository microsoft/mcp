// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Policy;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Policy;

/// <summary>
/// Consolidated policy command: when --policy is supplied returns a single policy's details;
/// otherwise lists all policies in the vault.
/// </summary>
public sealed class PolicyGetCommand(ILogger<PolicyGetCommand> logger) : BaseAzureBackupCommand<PolicyGetOptions>()
{
    private const string CommandTitle = "Get Backup Policy";
    private readonly ILogger<PolicyGetCommand> _logger = logger;

    public override string Id => "b1a2c3d4-e5f6-7890-abcd-ef1234567894";
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
        Destructive = false, Idempotent = true, OpenWorld = false,
        ReadOnly = true, LocalRequired = false, Secret = false
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
            var service = context.GetService<IAzureBackupService>();

            if (!string.IsNullOrEmpty(options.Policy))
            {
                // Single policy get
                var policy = await service.GetPolicyAsync(
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
                // List all policies
                var policies = await service.ListPoliciesAsync(
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
