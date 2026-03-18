// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.ProtectableItem;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureBackup.Commands.ProtectableItem;

public sealed class ProtectableItemListCommand(ILogger<ProtectableItemListCommand> logger) : BaseAzureBackupCommand<ProtectableItemListOptions>()
{
    private const string CommandTitle = "List Protectable Items";
    private readonly ILogger<ProtectableItemListCommand> _logger = logger;

    public override string Id => "c1a2b3c4-d5e6-7890-abcd-protectable001";
    public override string Name => "list";
    public override string Description =>
        """
        Lists protectable items (SQL databases, SAP HANA databases) discovered in the Recovery Services vault.
        Use after registering a container and running inquiry to discover databases available for protection.
        Filter results by workload type or container name. Valid workload-type values include:
        SAPHana, SAPHanaDatabase, SAPHanaSystem, SQL, SQLDataBase, SQLInstance.
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
        command.Options.Add(AzureBackupOptionDefinitions.WorkloadType);
        command.Options.Add(AzureBackupOptionDefinitions.Container);
    }

    protected override ProtectableItemListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkloadType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.WorkloadType.Name);
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
            var service = context.GetService<IAzureBackupService>();
            var result = await service.ListProtectableItemsAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.WorkloadType,
                options.Container,
                options.VaultType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ProtectableItemListCommandResult(result),
                AzureBackupJsonContext.Default.ProtectableItemListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing protectable items. Vault: {Vault}", options.Vault);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record ProtectableItemListCommandResult([property: JsonPropertyName("items")] List<ProtectableItemInfo> Items);
}
