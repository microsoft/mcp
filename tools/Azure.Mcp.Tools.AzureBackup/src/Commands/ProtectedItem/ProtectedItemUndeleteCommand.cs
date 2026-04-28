// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ProtectedItem;

public sealed class ProtectedItemUndeleteCommand(ILogger<ProtectedItemUndeleteCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<ProtectedItemUndeleteOptions>()
{
    private const string CommandTitle = "Undelete Protected Item";
    private readonly ILogger<ProtectedItemUndeleteCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "d8e3a1b7-5c42-4f9e-b6d1-7a2e9c3f4b58";
    public override string Name => "undelete";
    public override string Description =>
        """
        Undeletes or restores a soft-deleted backup item to an active protection state.
        Use this when a backup or protected item was accidentally deleted and needs to be recovered.
        For RSV vaults: pass the datasource ARM resource ID as --datasource-id.
        For DPP vaults: pass the datasource ARM resource ID as --datasource-id.
        Optionally specify --container for RSV workload items (SQL/HANA).
        The operation is asynchronous; use 'azurebackup job get' to monitor progress.
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
        command.Options.Add(AzureBackupOptionDefinitions.DatasourceId.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.Container);
    }

    protected override ProtectedItemUndeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DatasourceId = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.DatasourceId.Name);
        options.Container = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Container.Name);
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
            var result = await _azureBackupService.UndeleteProtectedItemAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.DatasourceId!,
                options.VaultType,
                options.Container,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.ProtectedItemUndeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error undeleting protected item. DatasourceId: {DatasourceId}, Vault: {Vault}",
                options.DatasourceId, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        KeyNotFoundException => "Soft-deleted protected item not found. Verify the datasource ID and vault.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Soft-deleted protected item not found. Verify the datasource ID, vault name, and resource group.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed undeleting the protected item. Ensure the caller has Backup Contributor role. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "This protected item is not in a soft-deleted state. Use 'azurebackup protecteditem get' to view its current status.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record ProtectedItemUndeleteCommandResult(OperationResult Result);
}
