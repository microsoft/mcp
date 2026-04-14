// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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

public sealed class GovernanceSoftDeleteCommand(ILogger<GovernanceSoftDeleteCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<GovernanceSoftDeleteOptions>()
{
    private const string CommandTitle = "Configure Soft Delete";
    private readonly ILogger<GovernanceSoftDeleteCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "b3f1ea2d-5535-4155-849c-61f2fc49f1d9";
    public override string Name => "soft-delete";
    public override string Description =>
        """
        Configures the soft delete settings for a backup vault. Set the state to 'AlwaysOn', 'On',
        or 'Off', and optionally specify the retention period in days (14-180).
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
        command.Options.Add(AzureBackupOptionDefinitions.SoftDelete.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.SoftDeleteRetentionDays);
        command.Validators.Add(commandResult =>
        {
            if (commandResult.HasOptionResult(AzureBackupOptionDefinitions.SoftDelete.Name))
            {
                var value = commandResult.GetValue<string>(AzureBackupOptionDefinitions.SoftDelete.Name);
                if (!string.IsNullOrEmpty(value) &&
                    !value.Equals("AlwaysOn", StringComparison.OrdinalIgnoreCase) &&
                    !value.Equals("On", StringComparison.OrdinalIgnoreCase) &&
                    !value.Equals("Off", StringComparison.OrdinalIgnoreCase))
                {
                    commandResult.AddError("--soft-delete must be 'AlwaysOn', 'On', or 'Off'.");
                }
            }

            if (commandResult.HasOptionResult(AzureBackupOptionDefinitions.SoftDeleteRetentionDays.Name))
            {
                var retentionValue = commandResult.GetValue<string>(AzureBackupOptionDefinitions.SoftDeleteRetentionDays.Name);
                if (!string.IsNullOrEmpty(retentionValue))
                {
                    if (!int.TryParse(retentionValue, out var retentionDays) || retentionDays < 14 || retentionDays > 180)
                    {
                        commandResult.AddError("--soft-delete-retention-days must be an integer between 14 and 180.");
                    }
                }
            }
        });
    }

    protected override GovernanceSoftDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SoftDeleteState = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.SoftDelete.Name);
        options.SoftDeleteRetentionDays = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.SoftDeleteRetentionDays.Name);
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
            var result = await _azureBackupService.ConfigureSoftDeleteAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.SoftDeleteState!,
                options.VaultType,
                options.SoftDeleteRetentionDays,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.GovernanceSoftDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring soft delete. Vault: {Vault}, State: {SoftDeleteState}",
                options.Vault, options.SoftDeleteState);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record GovernanceSoftDeleteCommandResult(OperationResult Result);
}
