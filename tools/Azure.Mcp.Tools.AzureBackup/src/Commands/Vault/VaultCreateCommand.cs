// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.Mcp.Tools.AzureBackup.Options;
using Azure.Mcp.Tools.AzureBackup.Options.Vault;
using Azure.Mcp.Tools.AzureBackup.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureBackup.Commands.Vault;

public sealed class VaultCreateCommand(ILogger<VaultCreateCommand> logger, IAzureBackupService azureBackupService) : BaseAzureBackupCommand<VaultCreateOptions>()
{
    private const string CommandTitle = "Create Backup Vault";
    private readonly ILogger<VaultCreateCommand> _logger = logger;
    private readonly IAzureBackupService _azureBackupService = azureBackupService;

    public override string Id => "1dccdb24-d81c-4bde-9437-577a7bd0cf09";
    public override string Name => "create";
    public override string Description =>
        """
        Creates a new backup vault. Specify --vault-type as 'rsv' for a Recovery Services vault
        or 'dpp' for a Backup vault (Data Protection). Returns the created vault details.
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
        command.Options.Add(AzureBackupOptionDefinitions.Location);
        command.Options.Add(AzureBackupOptionDefinitions.Sku);
        command.Options.Add(AzureBackupOptionDefinitions.StorageType);
        command.Validators.Add(commandResult =>
        {
            if (!commandResult.HasOptionResult(AzureBackupOptionDefinitions.VaultType.Name))
            {
                commandResult.AddError("--vault-type is required for vault creation. Specify 'rsv' or 'dpp'.");
            }
        });
    }

    protected override VaultCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Location = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Location.Name);
        options.Sku = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.Sku.Name);
        options.StorageType = parseResult.GetValueOrDefault<string>(AzureBackupOptionDefinitions.StorageType.Name);
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
            var result = await _azureBackupService.CreateVaultAsync(
                options.Vault!,
                options.ResourceGroup!,
                options.Subscription!,
                options.VaultType!,
                options.Location!,
                options.Sku,
                options.StorageType,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new VaultCreateCommandResult(result),
                AzureBackupJsonContext.Default.VaultCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vault. Vault: {Vault}, ResourceGroup: {ResourceGroup}, Location: {Location}",
                options.Vault, options.ResourceGroup, options.Location);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => argEx.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A vault with this name already exists. Choose a different name.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the vault. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record VaultCreateCommandResult([property: JsonPropertyName("vault")] VaultCreateResult Vault);
}
