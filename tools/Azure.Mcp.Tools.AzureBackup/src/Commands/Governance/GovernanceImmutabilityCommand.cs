// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Governance;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Governance;

public sealed class GovernanceImmutabilityCommand(ILogger<GovernanceImmutabilityCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<GovernanceImmutabilityOptions>()
{
    private const string CommandTitle = "Configure Vault Immutability";
    private readonly ILogger<GovernanceImmutabilityCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "a0ac7596-9a80-4b53-b459-06f27598a2e2";
    public override string Name => "immutability";
    public override string Description =>
        """
        Configures the immutability state for a backup vault. States include 'Disabled', 'Enabled',
        or 'Locked'. Warning: 'Locked' state is irreversible.
        """;
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.ImmutabilityState.AsRequired());
        command.Validators.Add(commandResult =>
        {
            if (commandResult.HasOptionResult(AzureBackupOptionDefinitions.ImmutabilityState.Name))
            {
                var value = commandResult.GetValue<string>(AzureBackupOptionDefinitions.ImmutabilityState.Name);
                if (!string.IsNullOrEmpty(value) &&
                    !value.Equals("Disabled", StringComparison.OrdinalIgnoreCase) &&
                    !value.Equals("Enabled", StringComparison.OrdinalIgnoreCase) &&
                    !value.Equals("Locked", StringComparison.OrdinalIgnoreCase))
                {
                    commandResult.AddError("--immutability-state must be 'Disabled', 'Enabled', or 'Locked'. Warning: 'Locked' is irreversible.");
                }
            }
        });
    }

    protected override GovernanceImmutabilityOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ImmutabilityState = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.ImmutabilityState.Name);
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

    internal record GovernanceImmutabilityCommandResult(OperationResult Result);
}
