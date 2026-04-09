// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
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

public sealed class ProtectedItemProtectCommand(ILogger<ProtectedItemProtectCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<ProtectedItemProtectOptions>()
{
    private const string CommandTitle = "Protect Resource";
    private readonly ILogger<ProtectedItemProtectCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "7a6fc193-ca3c-4309-97c5-ee1e7fe90e69";
    public override string Name => "protect";
    public override string Description =>
        """
        Enables backup protection for a resource by creating a protected item or backup instance.
        For VMs: pass the VM ARM resource ID as --datasource-id.
        For workloads (SQL/HANA): pass the protectable item name from 'protectableitem list' as --datasource-id
        (e.g., 'SAPHanaDatabase;instance;dbname'), and specify --container.
        Requires a backup policy name. The operation is asynchronous;
        use 'azurebackup job get' to monitor the protection job progress.
        """;
    public override string Title => CommandTitle;
    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureBackupOptionDefinitions.DatasourceId.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.Policy.AsRequired());
        command.Options.Add(AzureBackupOptionDefinitions.Container);
        command.Options.Add(AzureBackupOptionDefinitions.DatasourceType);
    }

    protected override ProtectedItemProtectOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.DatasourceId = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.DatasourceId.Name);
        options.Policy = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Policy.Name);
        options.Container = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Container.Name);
        options.DatasourceType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.DatasourceType.Name);
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
            var result = await _azureBackupService.ProtectItemAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.DatasourceId!,
                options.Policy!,
                options.VaultType,
                options.Container,
                options.DatasourceType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result),
                AzureBackupJsonContext.Default.ProtectedItemProtectCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error protecting item. DatasourceId: {DatasourceId}, Vault: {Vault}",
                options.DatasourceId, options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "This resource is already protected. Use 'azurebackup protecteditem get' to view its status.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record ProtectedItemProtectCommandResult(ProtectResult Result);
}
