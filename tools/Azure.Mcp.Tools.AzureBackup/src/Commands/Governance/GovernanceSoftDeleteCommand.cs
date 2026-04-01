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

public sealed class GovernanceSoftDeleteCommand(ILogger<GovernanceSoftDeleteCommand> logger) : BaseAzureBackupCommand<GovernanceSoftDeleteOptions>()
{
    private const string CommandTitle = "Configure Soft Delete";
    private readonly ILogger<GovernanceSoftDeleteCommand> _logger = logger;

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
            var service = context.GetService<IAzureBackupService>();
            var result = await service.ConfigureSoftDeleteAsync(
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
                new GovernanceSoftDeleteCommandResult(result),
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

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Vault not found. Verify the vault name and resource group.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record GovernanceSoftDeleteCommandResult([property: JsonPropertyName("result")] OperationResult Result);
}
